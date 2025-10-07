using Microsoft.Extensions.Logging;
using SoftFocusBackend.AI.Application.ACL.Services;
using SoftFocusBackend.AI.Domain.Model.ValueObjects;

namespace SoftFocusBackend.AI.Application.ACL.Implementations;

public class SubscriptionIntegrationService : ISubscriptionIntegrationService
{
    private readonly ILogger<SubscriptionIntegrationService> _logger;

    public SubscriptionIntegrationService(ILogger<SubscriptionIntegrationService> logger)
    {
        _logger = logger;
    }

    public Task<string> GetUserPlanAsync(string userId)
    {
        _logger.LogWarning("Subscriptions context not implemented yet. Returning 'Free' plan for user {UserId}", userId);
        return Task.FromResult("Free");
    }

    public Task<AIUsageLimit> GetPlanLimitsAsync(string plan)
    {
        _logger.LogWarning("Subscriptions context not implemented yet. Returning hardcoded limits for plan {Plan}", plan);

        var weekStart = GetMondayOfCurrentWeek();

        return Task.FromResult(plan.Equals("Premium", StringComparison.OrdinalIgnoreCase)
            ? AIUsageLimit.ForPremiumPlan(weekStart)
            : AIUsageLimit.ForFreePlan(weekStart));
    }

    private DateTime GetMondayOfCurrentWeek()
    {
        var now = DateTime.UtcNow;
        var daysSinceMonday = ((int)now.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return now.Date.AddDays(-daysSinceMonday);
    }
}
