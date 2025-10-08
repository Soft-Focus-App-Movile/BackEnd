namespace SoftFocusBackend.Tracking.Domain.Model.Queries;

public record GetEmotionalCalendarByDateRangeQuery
{
    public string UserId { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public DateTime RequestedAt { get; init; }

    public GetEmotionalCalendarByDateRangeQuery() { }

    public GetEmotionalCalendarByDateRangeQuery(string userId, DateTime startDate, DateTime endDate)
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        StartDate = startDate.Date;
        EndDate = endDate.Date;
        RequestedAt = DateTime.UtcNow;
    }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(UserId) &&
               StartDate <= EndDate;
    }
}