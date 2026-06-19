using SoftFocusBackend.Therapy.Domain.Model.Aggregates;
using SoftFocusBackend.Therapy.Domain.Model.Commands;
using SoftFocusBackend.Therapy.Domain.Model.ValueObjects;
using SoftFocusBackend.Therapy.Domain.Repositories;
using SoftFocusBackend.Therapy.Infrastructure.ExternalServices;

namespace SoftFocusBackend.Therapy.Application.Internal.CommandServices
{
    /// <summary>
    /// Ends a call (caller hangs up / cancels) or removes a single participant from a group call.
    /// Notifies the other participants over SignalR.
    /// </summary>
    public class EndCallCommandService
    {
        private readonly ICallSessionRepository _callRepository;
        private readonly SignalRCallService _signalRCallService;
        private readonly ILogger<EndCallCommandService> _logger;

        public EndCallCommandService(
            ICallSessionRepository callRepository,
            SignalRCallService signalRCallService,
            ILogger<EndCallCommandService> logger)
        {
            _callRepository = callRepository;
            _signalRCallService = signalRCallService;
            _logger = logger;
        }

        public async Task Handle(EndCallCommand command)
        {
            var call = await _callRepository.GetByIdAsync(command.CallId)
                ?? throw new InvalidOperationException("Call not found.");

            if (!call.IsParticipant(command.UserId))
                throw new UnauthorizedAccessException("You are not a participant of this call.");

            // In an ongoing group call, an invitee leaving only removes them; the call continues.
            var isInviteeLeavingGroup =
                call.Mode == CallMode.Group &&
                call.Status == CallStatus.Ongoing &&
                command.UserId != call.CallerId;

            if (isInviteeLeavingGroup)
            {
                call.Leave(command.UserId);
            }
            else
            {
                call.End(command.UserId);
            }

            await _callRepository.UpdateAsync(call);

            await NotifyOthersAsync(call, command.UserId);

            _logger.LogInformation(
                "Call {CallId} {Action} by {UserId}; status now {Status}",
                call.Id, isInviteeLeavingGroup ? "left" : "ended", command.UserId, call.Status);
        }

        private async Task NotifyOthersAsync(CallSession call, string actingUserId)
        {
            var payload = new
            {
                callId = call.Id,
                endedBy = actingUserId,
                status = call.Status.ToString(),
                endedAt = (call.EndedAt ?? DateTime.UtcNow).ToString("o")
            };

            var recipients = call.Participants
                .Select(p => p.UserId)
                .Where(id => id != actingUserId)
                .Distinct();

            foreach (var userId in recipients)
            {
                try { await _signalRCallService.NotifyCallEndedAsync(userId, payload); }
                catch (Exception ex) { _logger.LogError(ex, "Failed to notify {UserId} of call end {CallId}", userId, call.Id); }
            }
        }
    }
}
