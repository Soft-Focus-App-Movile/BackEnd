namespace SoftFocusBackend.Users.Domain.Model.Queries;

public record GetPsychologistStatsQuery
{
    public string UserId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public DateTime RequestedAt { get; init; }

    public GetPsychologistStatsQuery(string userId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        FromDate = fromDate;
        ToDate = toDate ?? DateTime.UtcNow;
        RequestedAt = DateTime.UtcNow;
    }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(UserId) &&
               (FromDate == null || ToDate == null || FromDate <= ToDate);
    }

    public string GetAuditString()
    {
        var parts = new List<string> { $"UserId: {UserId}" };

        if (FromDate.HasValue)
            parts.Add($"FromDate: {FromDate.Value:yyyy-MM-dd}");

        if (ToDate.HasValue)
            parts.Add($"ToDate: {ToDate.Value:yyyy-MM-dd}");

        parts.Add($"RequestedAt: {RequestedAt:yyyy-MM-dd HH:mm:ss} UTC");

        return string.Join(" | ", parts);
    }
}