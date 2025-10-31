namespace SoftFocusBackend.AI.Interfaces.REST.Resources;

public record ChatSessionResponse
{
    public string SessionId { get; init; } = string.Empty;
    public DateTime StartedAt { get; init; }
    public DateTime LastMessageAt { get; init; }
    public int MessageCount { get; init; }
    public bool IsActive { get; init; }
    public string? LastMessagePreview { get; init; }
}
