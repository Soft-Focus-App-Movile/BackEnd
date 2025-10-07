namespace SoftFocusBackend.AI.Domain.Model.ValueObjects;

public record AIUsageLimit
{
    public string Plan { get; init; }
    public int ChatLimit { get; init; }
    public int FacialLimit { get; init; }
    public DateTime ResetDate { get; init; }

    public AIUsageLimit(string plan, int chatLimit, int facialLimit, DateTime resetDate)
    {
        if (string.IsNullOrWhiteSpace(plan))
            throw new ArgumentException("Plan is required", nameof(plan));

        if (chatLimit < 0)
            throw new ArgumentException("ChatLimit cannot be negative", nameof(chatLimit));

        if (facialLimit < 0)
            throw new ArgumentException("FacialLimit cannot be negative", nameof(facialLimit));

        Plan = plan;
        ChatLimit = chatLimit;
        FacialLimit = facialLimit;
        ResetDate = resetDate;
    }

    public static AIUsageLimit ForFreePlan(DateTime weekStart)
    {
        return new AIUsageLimit("Free", 10, 3, weekStart.AddDays(7));
    }

    public static AIUsageLimit ForPremiumPlan(DateTime weekStart)
    {
        return new AIUsageLimit("Premium", 100, 30, weekStart.AddDays(7));
    }

    public bool IsPremium() => Plan.Equals("Premium", StringComparison.OrdinalIgnoreCase);
    public bool IsFree() => Plan.Equals("Free", StringComparison.OrdinalIgnoreCase);
}
