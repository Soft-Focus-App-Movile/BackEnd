using SoftFocusBackend.Subscription.Domain.ValueObjects;

namespace SoftFocusBackend.Subscription.Application.DTOs;

public class UsageStatsDto
{
    public FeatureType FeatureType { get; set; }
    public int CurrentUsage { get; set; }
    public int? Limit { get; set; }
    public bool IsUnlimited { get; set; }
    public bool LimitReached { get; set; }
    public int? Remaining { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime? LastUsedAt { get; set; }
}

public class AllUsageStatsDto
{
    public SubscriptionPlan Plan { get; set; }
    public List<UsageStatsDto> FeatureUsages { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}
