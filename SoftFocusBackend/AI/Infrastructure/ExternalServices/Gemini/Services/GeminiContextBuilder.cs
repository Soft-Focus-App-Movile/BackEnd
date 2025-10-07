using Microsoft.Extensions.Logging;
using SoftFocusBackend.AI.Application.ACL.Services;
using SoftFocusBackend.AI.Domain.Model.ValueObjects;
using SoftFocusBackend.AI.Domain.Repositories;
using SoftFocusBackend.AI.Domain.Services;

namespace SoftFocusBackend.AI.Infrastructure.ExternalServices.Gemini.Services;

public class GeminiContextBuilder : IGeminiContextBuilder
{
    private readonly IChatSessionRepository _chatSessionRepository;
    private readonly ITrackingIntegrationService _trackingService;
    private readonly ITherapyIntegrationService _therapyService;
    private readonly ILogger<GeminiContextBuilder> _logger;

    public GeminiContextBuilder(
        IChatSessionRepository chatSessionRepository,
        ITrackingIntegrationService trackingService,
        ITherapyIntegrationService therapyService,
        ILogger<GeminiContextBuilder> logger)
    {
        _chatSessionRepository = chatSessionRepository;
        _trackingService = trackingService;
        _therapyService = therapyService;
        _logger = logger;
    }

    public async Task<GeminiContext> BuildContextAsync(string userId, string message, string? sessionId)
    {
        _logger.LogInformation("Building Gemini context for user {UserId}", userId);

        var context = new GeminiContext(userId, message);

        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            var history = await _chatSessionRepository.GetSessionMessagesAsync(sessionId, limit: 10);
            context = context.WithHistory(history);
        }

        var recentCheckIns = await _trackingService.GetRecentCheckInsAsync(userId, days: 7);
        context = context.WithCheckIns(recentCheckIns);

        var therapyGoals = await _therapyService.GetCurrentTherapyGoalsAsync(userId);
        context = context.WithTherapyGoals(therapyGoals);

        var emotionalPattern = await _trackingService.GetEmotionalPatternAsync(userId);
        context = context.WithEmotionalPattern(emotionalPattern);

        _logger.LogInformation("Context built successfully: History={HasHistory}, CheckIns={CheckInCount}, Goals={GoalCount}",
            context.HasHistory(), context.RecentCheckIns.Count, context.TherapyGoals.Count);

        return context;
    }
}
