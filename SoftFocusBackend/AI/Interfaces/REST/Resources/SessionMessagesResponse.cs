namespace SoftFocusBackend.AI.Interfaces.REST.Resources;

public record SessionMessagesResponse
{
    public string SessionId { get; init; } = string.Empty;
    public List<ChatMessageItem> Messages { get; init; } = new();
}

public record ChatMessageItem
{
    public string Role { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public List<string> SuggestedQuestions { get; init; } = new();
}
