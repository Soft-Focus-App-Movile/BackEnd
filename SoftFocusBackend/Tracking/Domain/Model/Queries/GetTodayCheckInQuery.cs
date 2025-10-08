namespace SoftFocusBackend.Tracking.Domain.Model.Queries;

public record GetTodayCheckInQuery
{
    public string UserId { get; init; } = string.Empty;
    public DateTime RequestedAt { get; init; }

    public GetTodayCheckInQuery() { }

    public GetTodayCheckInQuery(string userId)
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        RequestedAt = DateTime.UtcNow;
    }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(UserId);
    }
}