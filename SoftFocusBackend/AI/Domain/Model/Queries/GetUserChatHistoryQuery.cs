namespace SoftFocusBackend.AI.Domain.Model.Queries;

public record GetUserChatHistoryQuery
{
    public string UserId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public int PageSize { get; init; }

    public GetUserChatHistoryQuery(string userId, DateTime? fromDate = null, DateTime? toDate = null, int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required", nameof(userId));

        if (pageSize <= 0 || pageSize > 100)
            throw new ArgumentException("PageSize must be between 1 and 100", nameof(pageSize));

        UserId = userId;
        FromDate = fromDate;
        ToDate = toDate;
        PageSize = pageSize;
    }
}
