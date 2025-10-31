using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftFocusBackend.AI.Application.Internal.CommandServices;
using SoftFocusBackend.AI.Application.Internal.QueryServices;
using SoftFocusBackend.AI.Domain.Model.Queries;
using SoftFocusBackend.AI.Interfaces.REST.Resources;
using SoftFocusBackend.AI.Interfaces.REST.Transform;
using SoftFocusBackend.AI.Domain.Model.ValueObjects;
using SoftFocusBackend.AI.Domain.Repositories;

namespace SoftFocusBackend.AI.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/ai/chat")]
[Authorize]
[Produces("application/json")]
public class AIChatController : ControllerBase
{
    private readonly AIChatCommandService _chatCommandService;
    private readonly AIUsageQueryService _usageQueryService;
    private readonly AIChatQueryService _chatQueryService;
    private readonly IChatSessionRepository _sessionRepository;
    private readonly ILogger<AIChatController> _logger;

    public AIChatController(
        AIChatCommandService chatCommandService,
        AIUsageQueryService usageQueryService,
        AIChatQueryService chatQueryService,
        IChatSessionRepository sessionRepository,
        ILogger<AIChatController> logger)
    {
        _chatCommandService = chatCommandService ?? throw new ArgumentNullException(nameof(chatCommandService));
        _usageQueryService = usageQueryService ?? throw new ArgumentNullException(nameof(usageQueryService));
        _chatQueryService = chatQueryService ?? throw new ArgumentNullException(nameof(chatQueryService));
        _sessionRepository = sessionRepository ?? throw new ArgumentNullException(nameof(sessionRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("message")]
    [ProducesResponseType(typeof(ChatMessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SendMessage([FromBody] ChatMessageRequest request)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("Unauthorized chat message attempt");
                return Unauthorized(AIResourceAssembler.ToErrorResponse("User not authenticated"));
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid chat message request from user {UserId}", userId);
                return BadRequest(AIResourceAssembler.ToErrorResponse("Invalid request data"));
            }

            var ipAddress = GetClientIpAddress();
            var command = AIResourceAssembler.ToCommand(request, userId, ipAddress);

            var result = await _chatCommandService.HandleSendMessageAsync(command);

            if (result.IsLimitExceeded)
            {
                _logger.LogWarning("User {UserId} exceeded chat usage limit", userId);
                return StatusCode(StatusCodes.Status429TooManyRequests,
                    AIResourceAssembler.ToErrorResponse("Chat usage limit exceeded. Please upgrade to Premium or wait for weekly reset.", result.Usage));
            }

            if (result.Response == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    AIResourceAssembler.ToErrorResponse("Failed to get chat response"));
            }

            var response = AIResourceAssembler.ToChatResponse(result.SessionId, result.Response);

            _logger.LogInformation("Chat message sent successfully for user {UserId}, session {SessionId}", userId, result.SessionId);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat message");
            return StatusCode(StatusCodes.Status500InternalServerError,
                AIResourceAssembler.ToErrorResponse("An error occurred while processing your message"));
        }
    }

    [HttpGet("usage")]
    [ProducesResponseType(typeof(AIUsageStatsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUsage()
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(AIResourceAssembler.ToErrorResponse("User not authenticated"));
            }

            var query = new GetAIUsageStatsQuery(userId);
            var usage = await _usageQueryService.HandleGetUsageStatsAsync(query);

            var response = AIResourceAssembler.ToUsageStatsResponse(usage);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage stats");
            return StatusCode(StatusCodes.Status500InternalServerError,
                AIResourceAssembler.ToErrorResponse("An error occurred while getting usage stats"));
        }
    }

    private string? GetUserId()
    {
        return User.FindFirst("user_id")?.Value;
    }

    private string? GetClientIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    [HttpGet("sessions")]
    [ProducesResponseType(typeof(ChatHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetChatSessions([FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(AIResourceAssembler.ToErrorResponse("User not authenticated"));
            }

            var query = new GetUserChatHistoryQuery(userId, pageSize: pageSize);
            var sessions = await _chatQueryService.HandleGetUserChatHistoryAsync(query);

            // Obtener previews de los últimos mensajes para cada sesión
            var sessionResponses = new List<ChatSessionResponse>();
            foreach (var session in sessions)
            {
                var preview = await _sessionRepository.GetLastUserMessagePreviewAsync(session.Id);
                sessionResponses.Add(new ChatSessionResponse
                {
                    SessionId = session.Id,
                    StartedAt = session.StartedAt,
                    LastMessageAt = session.LastMessageAt,
                    MessageCount = session.MessageCount,
                    IsActive = session.IsActive,
                    LastMessagePreview = preview
                });
            }

            var response = new ChatHistoryResponse
            {
                Sessions = sessionResponses,
                TotalCount = sessions.Count
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting chat sessions");
            return StatusCode(StatusCodes.Status500InternalServerError,
                AIResourceAssembler.ToErrorResponse("An error occurred while getting chat sessions"));
        }
    }

    [HttpGet("sessions/{sessionId}/messages")]
    [ProducesResponseType(typeof(SessionMessagesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSessionMessages(string sessionId, [FromQuery] int limit = 50)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(AIResourceAssembler.ToErrorResponse("User not authenticated"));
            }

            var session = await _chatQueryService.HandleGetChatSessionAsync(new GetChatSessionQuery(sessionId, userId));
            if (session == null || session.UserId != userId)
            {
                return NotFound(AIResourceAssembler.ToErrorResponse("Session not found"));
            }

            var messages = await _chatQueryService.GetSessionMessagesAsync(sessionId, limit);

            var response = new SessionMessagesResponse
            {
                SessionId = sessionId,
                Messages = messages.OrderBy(m => m.Timestamp).Select(m => new ChatMessageItem
                {
                    Role = m.Role == ChatRole.User ? "user" : "assistant",
                    Content = m.Content,
                    Timestamp = m.Timestamp,
                    SuggestedQuestions = m.SuggestedQuestions
                }).ToList()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session messages");
            return StatusCode(StatusCodes.Status500InternalServerError,
                AIResourceAssembler.ToErrorResponse("An error occurred while getting session messages"));
        }
    }
}
