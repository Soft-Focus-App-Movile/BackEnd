using Microsoft.Extensions.Logging;
using SoftFocusBackend.AI.Application.ACL.Services;
using SoftFocusBackend.AI.Domain.Model.Aggregates;
using SoftFocusBackend.AI.Domain.Model.ValueObjects;
using SoftFocusBackend.AI.Domain.Repositories;
using SoftFocusBackend.AI.Domain.Services;
using System.Globalization;

namespace SoftFocusBackend.AI.Infrastructure.ExternalServices.UsageTracking;

public class AIUsageTrackerService : IAIUsageTracker
{
    private readonly IAIUsageRepository _usageRepository;
    private readonly ISubscriptionIntegrationService _subscriptionService;
    private readonly ILogger<AIUsageTrackerService> _logger;

    public AIUsageTrackerService(
        IAIUsageRepository usageRepository,
        ISubscriptionIntegrationService subscriptionService,
        ILogger<AIUsageTrackerService> logger)
    {
        _usageRepository = usageRepository;
        _subscriptionService = subscriptionService;
        _logger = logger;
    }

    public async Task<bool> CanUseChatAsync(string userId)
    {
        var usage = await GetOrCreateCurrentUsageAsync(userId);
        return usage.CanUseChat();
    }

    public async Task<bool> CanUseFacialAnalysisAsync(string userId)
    {
        var usage = await GetOrCreateCurrentUsageAsync(userId);
        return usage.CanUseFacialAnalysis();
    }

    public async Task IncrementChatUsageAsync(string userId)
    {
        await _usageRepository.IncrementUsageAsync(userId, "chat");
        _logger.LogInformation("Incremented chat usage for user {UserId}", userId);
    }

    public async Task IncrementFacialUsageAsync(string userId)
    {
        await _usageRepository.IncrementUsageAsync(userId, "facial");
        _logger.LogInformation("Incremented facial analysis usage for user {UserId}", userId);
    }

    public async Task<AIUsage> GetCurrentUsageAsync(string userId)
    {
        return await GetOrCreateCurrentUsageAsync(userId);
    }

    private async Task<AIUsage> GetOrCreateCurrentUsageAsync(string userId)
    {
        var existing = await _usageRepository.GetCurrentWeekUsageAsync(userId);
        if (existing != null)
        {
            return existing;
        }

        var plan = await _subscriptionService.GetUserPlanAsync(userId);
        var limits = await _subscriptionService.GetPlanLimitsAsync(plan);

        var (week, weekStart) = GetCurrentWeek();

        var newUsage = AIUsage.Create(
            userId,
            week,
            weekStart,
            plan,
            limits.ChatLimit,
            limits.FacialLimit
        );

        return await _usageRepository.CreateOrUpdateAsync(newUsage);
    }

    private (string week, DateTime weekStart) GetCurrentWeek()
    {
        var now = DateTime.UtcNow;
        var daysSinceMonday = ((int)now.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        var weekStart = now.Date.AddDays(-daysSinceMonday);

        var calendar = CultureInfo.InvariantCulture.Calendar;
        var weekNumber = calendar.GetWeekOfYear(weekStart, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        var week = $"{weekStart.Year}-W{weekNumber:D2}";

        return (week, weekStart);
    }
}
