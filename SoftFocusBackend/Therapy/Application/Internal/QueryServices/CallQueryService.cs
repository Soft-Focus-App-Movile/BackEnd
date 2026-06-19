using SoftFocusBackend.Therapy.Application.Internal.CommandServices;
using SoftFocusBackend.Therapy.Domain.Model.Aggregates;
using SoftFocusBackend.Therapy.Domain.Model.Queries;
using SoftFocusBackend.Therapy.Domain.Repositories;
using SoftFocusBackend.Therapy.Domain.Services;

namespace SoftFocusBackend.Therapy.Application.Internal.QueryServices
{
    /// <summary>
    /// Read-side operations for calls: token renewal and call history.
    /// </summary>
    public class CallQueryService
    {
        private readonly ICallSessionRepository _callRepository;
        private readonly IAgoraTokenService _agoraTokenService;

        public CallQueryService(
            ICallSessionRepository callRepository,
            IAgoraTokenService agoraTokenService)
        {
            _callRepository = callRepository;
            _agoraTokenService = agoraTokenService;
        }

        /// <summary>Re-issues a token for a participant of an existing call (token renewal / reconnect).</summary>
        public async Task<CallAccessResult> Handle(GetCallTokenQuery query)
        {
            var call = await _callRepository.GetByIdAsync(query.CallId)
                ?? throw new InvalidOperationException("Call not found.");

            if (!call.IsParticipant(query.UserId))
                throw new UnauthorizedAccessException("You are not a participant of this call.");

            var token = _agoraTokenService.GenerateRtcToken(call.ChannelName, query.UserId, call.CallType);
            return new CallAccessResult(call, _agoraTokenService.AppId, token, query.UserId);
        }

        public async Task<IEnumerable<CallSession>> Handle(GetCallHistoryQuery query)
        {
            return await _callRepository.GetByParticipantIdAsync(query.UserId, query.Page, query.Size);
        }
    }
}
