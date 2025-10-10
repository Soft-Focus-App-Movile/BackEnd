namespace SoftFocusBackend.Tracking.Domain.Model.Queries;

public record GetUserCheckInsQuery
{
    public string UserId { get; init; } = string.Empty;
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public DateTime RequestedAt { get; init; }

    public GetUserCheckInsQuery() { }

    public GetUserCheckInsQuery(string userId, DateTime? startDate = null, DateTime? endDate = null, 
        int pageNumber = 1, int pageSize = 20)
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        StartDate = startDate;
        EndDate = endDate;
        PageNumber = pageNumber > 0 ? pageNumber : 1;
        PageSize = pageSize > 0 && pageSize <= 100 ? pageSize : 20;
        RequestedAt = DateTime.UtcNow;
    }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(UserId) &&
               PageNumber > 0 &&
               PageSize > 0 && PageSize <= 100;
    }
}