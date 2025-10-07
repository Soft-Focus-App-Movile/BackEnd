using Microsoft.Extensions.Logging;
using SoftFocusBackend.AI.Application.ACL.Services;
using SoftFocusBackend.AI.Domain.Model.Aggregates;
using SoftFocusBackend.AI.Domain.Model.Commands;
using SoftFocusBackend.AI.Domain.Repositories;
using SoftFocusBackend.AI.Domain.Services;

namespace SoftFocusBackend.AI.Application.Internal.CommandServices;

public class AIChatCommandService
{
    private readonly IAIUsageTracker _usageTracker;
    private readonly IGeminiContextBuilder _contextBuilder;
    private readonly IEmotionalChatService _chatService;
    private readonly ICrisisPatternDetector _crisisDetector;
    private readonly ICrisisIntegrationService _crisisIntegration;
    private readonly IChatSessionRepository _sessionRepository;
    private readonly ILogger<AIChatCommandService> _logger;

    public AIChatCommandService(
        IAIUsageTracker usageTracker,
        IGeminiContextBuilder contextBuilder,
        IEmotionalChatService chatService,
        ICrisisPatternDetector crisisDetector,
        ICrisisIntegrationService crisisIntegration,
        IChatSessionRepository sessionRepository,
        ILogger<AIChatCommandService> logger)
    {
        _usageTracker = usageTracker;
        _contextBuilder = contextBuilder;
        _chatService = chatService;
        _crisisDetector = crisisDetector;
        _crisisIntegration = crisisIntegration;
        _sessionRepository = sessionRepository;
        _logger = logger;
    }

    public async Task<ChatCommandResult> HandleSendMessageAsync(SendChatMessageCommand command)
    {
        try
        {
            _logger.LogInformation(command.GetAuditString());

            var canUse = await _usageTracker.CanUseChatAsync(command.UserId);
            if (!canUse)
            {
                _logger.LogWarning("User {UserId} exceeded chat usage limit", command.UserId);
                var usage = await _usageTracker.GetCurrentUsageAsync(command.UserId);
                return ChatCommandResult.LimitExceeded(usage);
            }

            var session = await GetOrCreateSessionAsync(command.UserId, command.SessionId);

            var userMessage = ChatMessage.CreateUserMessage(session.Id, command.Message);
            await _sessionRepository.AddMessageAsync(session.Id, userMessage);

            var context = await _contextBuilder.BuildContextAsync(command.UserId, command.Message, session.Id);

            var response = await _chatService.SendMessageAsync(context);

            var assistantMessage = ChatMessage.CreateAssistantMessage(
                session.Id,
                response.Reply,
                response.SuggestedQuestions,
                response.RecommendedExercises,
                response.CrisisDetected
            );

            await _sessionRepository.AddMessageAsync(session.Id, assistantMessage);

            var crisisPattern = await _crisisDetector.DetectFromChatAsync(userMessage);
            if (crisisPattern != null || response.CrisisDetected)
            {
                var severity = crisisPattern?.GetSeverityString() ?? "moderate";
                var reason = crisisPattern?.TriggerReason ?? response.CrisisContext;

                await _crisisIntegration.TriggerCrisisAlertAsync(new CrisisAlertRequest
                {
                    UserId = command.UserId,
                    Source = "ai_chat",
                    Severity = severity,
                    TriggerReason = reason,
                    Context = $"User message: {command.Message.Substring(0, Math.Min(100, command.Message.Length))}...",
                    DetectedAt = DateTime.UtcNow
                });
            }

            await _usageTracker.IncrementChatUsageAsync(command.UserId);

            return ChatCommandResult.Success(session.Id, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling send message command for user {UserId}", command.UserId);
            throw;
        }
    }

    private async Task<ChatSession> GetOrCreateSessionAsync(string userId, string? sessionId)
    {
        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            var existing = await _sessionRepository.GetByIdAsync(sessionId);
            if (existing != null && existing.UserId == userId)
            {
                return existing;
            }
        }

        return await _sessionRepository.CreateAsync(userId);
    }
}

public record ChatCommandResult
{
    public bool IsSuccess { get; init; }
    public bool IsLimitExceeded { get; init; }
    public string SessionId { get; init; } = string.Empty;
    public ChatResponse? Response { get; init; }
    public AIUsage? Usage { get; init; }

    public static ChatCommandResult Success(string sessionId, ChatResponse response)
    {
        return new ChatCommandResult
        {
            IsSuccess = true,
            IsLimitExceeded = false,
            SessionId = sessionId,
            Response = response
        };
    }

    public static ChatCommandResult LimitExceeded(AIUsage usage)
    {
        return new ChatCommandResult
        {
            IsSuccess = false,
            IsLimitExceeded = true,
            Usage = usage
        };
    }
}
