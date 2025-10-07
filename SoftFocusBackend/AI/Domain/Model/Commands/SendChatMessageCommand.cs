namespace SoftFocusBackend.AI.Domain.Model.Commands;

public record SendChatMessageCommand
{
    public string UserId { get; init; }
    public string Message { get; init; }
    public string? SessionId { get; init; }
    public DateTime RequestedAt { get; init; }
    public string? IpAddress { get; init; }

    public SendChatMessageCommand(string userId, string message, string? sessionId = null,
        string? ipAddress = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required", nameof(userId));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message is required", nameof(message));

        if (message.Length > 2000)
            throw new ArgumentException("Message cannot exceed 2000 characters", nameof(message));

        UserId = userId;
        Message = message;
        SessionId = sessionId;
        RequestedAt = DateTime.UtcNow;
        IpAddress = ipAddress;
    }

    public string GetAuditString()
    {
        return $"User {UserId} sent chat message (Session: {SessionId ?? "new"}) at {RequestedAt:yyyy-MM-dd HH:mm:ss} from {IpAddress ?? "unknown"}";
    }
}
