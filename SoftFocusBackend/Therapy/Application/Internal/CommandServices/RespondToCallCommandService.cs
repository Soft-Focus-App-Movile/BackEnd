using SoftFocusBackend.Therapy.Application.Internal.OutboundServices;
using SoftFocusBackend.Therapy.Domain.Model.Commands;
using SoftFocusBackend.Therapy.Domain.Repositories;
using SoftFocusBackend.Therapy.Domain.Services;
using SoftFocusBackend.Therapy.Infrastructure.ExternalServices;

namespace SoftFocusBackend.Therapy.Application.Internal.CommandServices
{
    /// <summary>
    /// Handles an invitee accepting or rejecting an incoming call. On accept, returns the invitee's
    /// Agora token so they can join; the caller is notified either way over SignalR.
    /// </summary>
    public class RespondToCallCommandService
    {
        private readonly ICallSessionRepository _callRepository;
        private readonly IAgoraTokenService _agoraTokenService;
        private readonly IPatientFacade _patientFacade;
        private readonly SignalRCallService _signalRCallService;
        private readonly ILogger<RespondToCallCommandService> _logger;

        public RespondToCallCommandService(
            ICallSessionRepository callRepository,
            IAgoraTokenService agoraTokenService,
            IPatientFacade patientFacade,
            SignalRCallService signalRCallService,
            ILogger<RespondToCallCommandService> logger)
        {
            _callRepository = callRepository;
            _agoraTokenService = agoraTokenService;
            _patientFacade = patientFacade;
            _signalRCallService = signalRCallService;
            _logger = logger;
        }

        /// <returns>The join credentials when accepted; null when rejected.</returns>
        public async Task<CallAccessResult?> Handle(RespondToCallCommand command)
        {
            var call = await _callRepository.GetByIdAsync(command.CallId)
                ?? throw new InvalidOperationException("Call not found.");

            if (!call.IsParticipant(command.UserId))
                throw new UnauthorizedAccessException("You are not a participant of this call.");

            var responder = await _patientFacade.FetchUserById(command.UserId);
            var responderName = responder?.FullName ?? command.UserId;

            if (command.Accept)
            {
                call.Accept(command.UserId);
                await _callRepository.UpdateAsync(call);

                var token = _agoraTokenService.GenerateRtcToken(call.ChannelName, command.UserId, call.CallType);

                await SafeNotify(() => _signalRCallService.NotifyCallAcceptedAsync(call.CallerId, new
                {
                    callId = call.Id,
                    channelName = call.ChannelName,
                    user = new { id = command.UserId, name = responderName }
                }), call.Id);

                _logger.LogInformation("Call {CallId} accepted by {UserId}", call.Id, command.UserId);
                return new CallAccessResult(call, _agoraTokenService.AppId, token, command.UserId);
            }

            call.Reject(command.UserId);
            await _callRepository.UpdateAsync(call);

            await SafeNotify(() => _signalRCallService.NotifyCallRejectedAsync(call.CallerId, new
            {
                callId = call.Id,
                user = new { id = command.UserId, name = responderName },
                status = call.Status.ToString()
            }), call.Id);

            _logger.LogInformation("Call {CallId} rejected by {UserId}", call.Id, command.UserId);
            return null;
        }

        private async Task SafeNotify(Func<Task> notify, string callId)
        {
            try { await notify(); }
            catch (Exception ex) { _logger.LogError(ex, "SignalR notify failed for call {CallId}", callId); }
        }
    }
}
