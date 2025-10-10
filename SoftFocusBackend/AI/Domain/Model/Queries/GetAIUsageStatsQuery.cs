namespace SoftFocusBackend.AI.Domain.Model.Queries;

public record GetAIUsageStatsQuery
{
    public string UserId { get; init; }
    public string? Week { get; init; }

    public GetAIUsageStatsQuery(string userId, string? week = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required", nameof(userId));

        UserId = userId;
        Week = week;
    }
}
