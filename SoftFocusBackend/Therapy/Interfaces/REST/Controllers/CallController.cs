using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftFocusBackend.Therapy.Application.Internal.CommandServices;
using SoftFocusBackend.Therapy.Application.Internal.QueryServices;
using SoftFocusBackend.Therapy.Domain.Model.Aggregates;
using SoftFocusBackend.Therapy.Domain.Model.Commands;
using SoftFocusBackend.Therapy.Domain.Model.Queries;
using SoftFocusBackend.Therapy.Domain.Model.ValueObjects;
using SoftFocusBackend.Therapy.Interfaces.REST.Resources;
using Swashbuckle.AspNetCore.Annotations;

namespace SoftFocusBackend.Therapy.Interfaces.REST.Controllers
{
    /// <summary>
    /// Voice/video calls between patients and psychologists, powered by Agora.io.
    /// Signaling (ringing, accept/reject/end) is delivered in real time over the SignalR "/callHub";
    /// the actual media is carried by the Agora SDK on the client using the channel name + token
    /// returned here.
    /// </summary>
    [ApiController]
    [Route("api/v1/calls")]
    [Authorize]
    [Produces("application/json")]
    public class CallController : ControllerBase
    {
        private readonly InitiateCallCommandService _initiateService;
        private readonly RespondToCallCommandService _respondService;
        private readonly EndCallCommandService _endService;
        private readonly CallQueryService _callQueryService;

        public CallController(
            InitiateCallCommandService initiateService,
            RespondToCallCommandService respondService,
            EndCallCommandService endService,
            CallQueryService callQueryService)
        {
            _initiateService = initiateService;
            _respondService = respondService;
            _endService = endService;
            _callQueryService = callQueryService;
        }

        [HttpPost("initiate")]
        [SwaggerOperation(
            Summary = "Start a call",
            Description = "Patients call their psychologist; psychologists call a single patient (Direct) " +
                          "or all of their active patients (Group). Returns the Agora channel, appId and a " +
                          "token for the caller, and rings the invitees over SignalR.",
            OperationId = "InitiateCall",
            Tags = new[] { "Calls" })]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Initiate([FromBody] InitiateCallRequest request)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { error = "User not authenticated" });

            if (!TryParseCallType(request.CallType, out var callType))
                return BadRequest(new { error = "CallType must be 'Audio' or 'Video'." });
            if (!TryParseCallMode(request.Mode, out var mode))
                return BadRequest(new { error = "Mode must be 'Direct' or 'Group'." });

            var command = new InitiateCallCommand
            {
                CallerId = userId,
                CallType = callType,
                Mode = mode,
                TargetUserId = request.TargetUserId
            };

            try
            {
                var result = await _initiateService.Handle(command);
                return Ok(ToAccessResponse(result));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{callId}/answer")]
        [SwaggerOperation(
            Summary = "Answer a call",
            Description = "An invitee accepts the call and receives their Agora token to join the channel. " +
                          "The caller is notified via SignalR ('CallAccepted').",
            OperationId = "AnswerCall",
            Tags = new[] { "Calls" })]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Answer(string callId)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { error = "User not authenticated" });

            try
            {
                var result = await _respondService.Handle(new RespondToCallCommand
                {
                    CallId = callId,
                    UserId = userId,
                    Accept = true
                });
                return Ok(ToAccessResponse(result!));
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (InvalidOperationException ex) { return NotFound(new { error = ex.Message }); }
        }

        [HttpPost("{callId}/reject")]
        [SwaggerOperation(
            Summary = "Reject a call",
            Description = "An invitee declines the call. The caller is notified via SignalR ('CallRejected').",
            OperationId = "RejectCall",
            Tags = new[] { "Calls" })]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Reject(string callId)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { error = "User not authenticated" });

            try
            {
                await _respondService.Handle(new RespondToCallCommand
                {
                    CallId = callId,
                    UserId = userId,
                    Accept = false
                });
                return Ok(new { message = "Call rejected" });
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (InvalidOperationException ex) { return NotFound(new { error = ex.Message }); }
        }

        [HttpPost("{callId}/end")]
        [SwaggerOperation(
            Summary = "End or leave a call",
            Description = "Hangs up the call. The caller ending a call ends it for everyone; an invitee " +
                          "leaving an ongoing group call only removes themselves. Other participants are " +
                          "notified via SignalR ('CallEnded').",
            OperationId = "EndCall",
            Tags = new[] { "Calls" })]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> End(string callId)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { error = "User not authenticated" });

            try
            {
                await _endService.Handle(new EndCallCommand { CallId = callId, UserId = userId });
                return Ok(new { message = "Call ended" });
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (InvalidOperationException ex) { return NotFound(new { error = ex.Message }); }
        }

        [HttpGet("{callId}/token")]
        [SwaggerOperation(
            Summary = "Renew an Agora token",
            Description = "Re-issues a fresh Agora token for the authenticated participant of an existing " +
                          "call. Use on the SDK token-privilege-will-expire callback.",
            OperationId = "RenewCallToken",
            Tags = new[] { "Calls" })]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RenewToken(string callId)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { error = "User not authenticated" });

            try
            {
                var result = await _callQueryService.Handle(new GetCallTokenQuery { CallId = callId, UserId = userId });
                return Ok(ToAccessResponse(result));
            }
            catch (UnauthorizedAccessException ex) { return Forbid(ex.Message); }
            catch (InvalidOperationException ex) { return NotFound(new { error = ex.Message }); }
        }

        [HttpGet("history")]
        [SwaggerOperation(
            Summary = "Get call history",
            Description = "Paginated history of calls the authenticated user took part in, newest first.",
            OperationId = "GetCallHistory",
            Tags = new[] { "Calls" })]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> History(int page = 1, int size = 20)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { error = "User not authenticated" });

            var calls = await _callQueryService.Handle(new GetCallHistoryQuery { UserId = userId, Page = page, Size = size });
            return Ok(calls.Select(ToHistoryItem));
        }

        private static object ToAccessResponse(CallAccessResult result) => new
        {
            callId = result.Session.Id,
            channelName = result.Session.ChannelName,
            appId = result.AppId,
            token = result.Token,
            userAccount = result.UserAccount,
            callType = result.Session.CallType.ToString(),
            mode = result.Session.Mode.ToString(),
            status = result.Session.Status.ToString(),
            inviteeIds = result.Session.InviteeIds
        };

        private static object ToHistoryItem(CallSession call) => new
        {
            callId = call.Id,
            channelName = call.ChannelName,
            callerId = call.CallerId,
            callerRole = call.CallerRole,
            callType = call.CallType.ToString(),
            mode = call.Mode.ToString(),
            status = call.Status.ToString(),
            startedAt = call.StartedAt,
            answeredAt = call.AnsweredAt,
            endedAt = call.EndedAt,
            participants = call.Participants.Select(p => new
            {
                userId = p.UserId,
                isCaller = p.IsCaller,
                hasAccepted = p.HasAccepted,
                hasRejected = p.HasRejected,
                joinedAt = p.JoinedAt,
                leftAt = p.LeftAt
            })
        };

        private static bool TryParseCallType(string value, out CallType callType) =>
            Enum.TryParse(value, ignoreCase: true, out callType);

        private static bool TryParseCallMode(string value, out CallMode mode) =>
            Enum.TryParse(value, ignoreCase: true, out mode);

        private string? GetCurrentUserId() => User.FindFirst("user_id")?.Value;
    }
}
