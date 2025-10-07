namespace SoftFocusBackend.AI.Domain.Model.Queries;

public record GetChatSessionQuery
{
    public string SessionId { get; init; }
    public string UserId { get; init; }

    public GetChatSessionQuery(string sessionId, string userId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("SessionId is required", nameof(sessionId));

        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required", nameof(userId));

        SessionId = sessionId;
        UserId = userId;
    }
}
