namespace SoftFocusBackend.Tracking.Domain.Model.Queries;

public record GetEmotionalCalendarEntryByDateQuery
{
    public string UserId { get; init; } = string.Empty;
    public DateTime Date { get; init; }
    public DateTime RequestedAt { get; init; }

    public GetEmotionalCalendarEntryByDateQuery() { }

    public GetEmotionalCalendarEntryByDateQuery(string userId, DateTime date)
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        Date = date.Date;
        RequestedAt = DateTime.UtcNow;
    }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(UserId);
    }
}