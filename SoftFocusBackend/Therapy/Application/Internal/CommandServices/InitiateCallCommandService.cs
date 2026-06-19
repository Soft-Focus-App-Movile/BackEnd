using SoftFocusBackend.Therapy.Application.Internal.OutboundServices;
using SoftFocusBackend.Therapy.Domain.Model.Aggregates;
using SoftFocusBackend.Therapy.Domain.Model.Commands;
using SoftFocusBackend.Therapy.Domain.Model.ValueObjects;
using SoftFocusBackend.Therapy.Domain.Repositories;
using SoftFocusBackend.Therapy.Domain.Services;
using SoftFocusBackend.Therapy.Infrastructure.ExternalServices;

namespace SoftFocusBackend.Therapy.Application.Internal.CommandServices
{
    /// <summary>
    /// Creates a call, persists it, generates the caller's Agora token and rings the invitees over SignalR.
    /// </summary>
    public class InitiateCallCommandService
    {
        private readonly ITherapeuticRelationshipRepository _relationshipRepository;
        private readonly ICallSessionRepository _callRepository;
        private readonly IAgoraTokenService _agoraTokenService;
        private readonly IPatientFacade _patientFacade;
        private readonly SignalRCallService _signalRCallService;
        private readonly ILogger<InitiateCallCommandService> _logger;

        public InitiateCallCommandService(
            ITherapeuticRelationshipRepository relationshipRepository,
            ICallSessionRepository callRepository,
            IAgoraTokenService agoraTokenService,
            IPatientFacade patientFacade,
            SignalRCallService signalRCallService,
            ILogger<InitiateCallCommandService> logger)
        {
            _relationshipRepository = relationshipRepository;
            _callRepository = callRepository;
            _agoraTokenService = agoraTokenService;
            _patientFacade = patientFacade;
            _signalRCallService = signalRCallService;
            _logger = logger;
        }

        public async Task<CallAccessResult> Handle(InitiateCallCommand command)
        {
            var caller = await _patientFacade.FetchUserById(command.CallerId)
                ?? throw new InvalidOperationException("Caller not found.");

            // Prevent the same user from starting more than one call at a time.
            var activeCalls = await _callRepository.GetActiveByUserIdAsync(command.CallerId);
            if (activeCalls.Any())
                throw new InvalidOperationException("You already have a call in progress.");

            var isPsychologist = caller.IsPsychologist();
            var callerRole = isPsychologist ? "Psychologist" : "Patient";

            string? relationshipId;
            CallMode mode;
            List<string> inviteeIds;

            if (isPsychologist)
            {
                if (command.Mode == CallMode.Group)
                {
                    mode = CallMode.Group;
                    relationshipId = null;

                    var relationships = await _relationshipRepository.GetByPsychologistIdAsync(command.CallerId);
                    inviteeIds = relationships
                        .Where(r => r.Status == TherapyStatus.Active && r.IsActive)
                        .Select(r => r.PatientId)
                        .Where(id => !string.IsNullOrWhiteSpace(id))
                        .Distinct()
                        .ToList();

                    if (inviteeIds.Count == 0)
                        throw new InvalidOperationException("You have no active patients to call.");
                }
                else
                {
                    mode = CallMode.Direct;
                    if (string.IsNullOrWhiteSpace(command.TargetUserId))
                        throw new InvalidOperationException("TargetUserId is required for a direct call.");

                    var relationship = await FindActiveRelationship(command.CallerId, command.TargetUserId);
                    relationshipId = relationship.Id;
                    inviteeIds = new List<string> { command.TargetUserId };
                }
            }
            else
            {
                // Patient → always a direct call to their psychologist.
                mode = CallMode.Direct;
                var relationships = await _relationshipRepository.GetByPatientIdAsync(command.CallerId);
                var relationship = relationships.FirstOrDefault(r => r.Status == TherapyStatus.Active && r.IsActive)
                    ?? throw new InvalidOperationException("You don't have an active psychologist to call.");

                relationshipId = relationship.Id;
                inviteeIds = new List<string> { relationship.PsychologistId };
            }

            var channelName = $"call_{Guid.NewGuid():N}";

            var call = new CallSession(
                channelName,
                command.CallerId,
                callerRole,
                command.CallType,
                mode,
                relationshipId,
                inviteeIds);

            await _callRepository.AddAsync(call);

            var token = _agoraTokenService.GenerateRtcToken(channelName, command.CallerId, command.CallType);

            await RingInviteesAsync(call, caller.FullName, callerRole);

            _logger.LogInformation(
                "Call {CallId} initiated by {CallerId} ({Role}, {Mode}) ringing {Count} invitee(s) on channel {Channel}",
                call.Id, command.CallerId, callerRole, mode, inviteeIds.Count, channelName);

            return new CallAccessResult(call, _agoraTokenService.AppId, token, command.CallerId);
        }

        private async Task<Domain.Model.Aggregates.TherapeuticRelationship> FindActiveRelationship(
            string psychologistId, string patientId)
        {
            var relationships = await _relationshipRepository.GetByPsychologistIdAsync(psychologistId);
            var relationship = relationships.FirstOrDefault(r =>
                r.PatientId == patientId && r.Status == TherapyStatus.Active && r.IsActive);

            return relationship
                ?? throw new InvalidOperationException("No active relationship with the target patient.");
        }

        private async Task RingInviteesAsync(CallSession call, string callerName, string callerRole)
        {
            var payload = new
            {
                callId = call.Id,
                channelName = call.ChannelName,
                callType = call.CallType.ToString(),
                mode = call.Mode.ToString(),
                appId = _agoraTokenService.AppId,
                caller = new { id = call.CallerId, name = callerName, role = callerRole },
                startedAt = call.StartedAt.ToString("o")
            };

            foreach (var inviteeId in call.InviteeIds)
            {
                try
                {
                    await _signalRCallService.NotifyIncomingCallAsync(inviteeId, payload);
                }
                catch (Exception ex)
                {
                    // A failed ring shouldn't abort the whole call; the invitee can still be reached
                    // via push/polling. Log and continue.
                    _logger.LogError(ex,
                        "Failed to ring invitee {InviteeId} for call {CallId}", inviteeId, call.Id);
                }
            }
        }
    }
}
