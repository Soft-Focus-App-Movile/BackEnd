namespace SoftFocusBackend.Tracking.Domain.Model.Queries;

public record GetUserEmotionalCalendarQuery
{
    public string UserId { get; init; } = string.Empty;
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 30;
    public DateTime RequestedAt { get; init; }

    public GetUserEmotionalCalendarQuery() { }

    public GetUserEmotionalCalendarQuery(string userId, int pageNumber = 1, int pageSize = 30)
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        PageNumber = pageNumber > 0 ? pageNumber : 1;
        PageSize = pageSize > 0 && pageSize <= 100 ? pageSize : 30;
        RequestedAt = DateTime.UtcNow;
    }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(UserId) &&
               PageNumber > 0 &&
               PageSize > 0 && PageSize <= 100;
    }
}