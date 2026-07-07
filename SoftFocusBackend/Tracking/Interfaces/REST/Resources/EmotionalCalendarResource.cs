namespace SoftFocusBackend.Tracking.Interfaces.REST.Resources;

public record EmotionalCalendarResource
{
    public string Id { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public DateTime Date { get; init; }
    public string EmotionalEmoji { get; init; } = string.Empty;
    public int MoodLevel { get; init; }
    public List<string> EmotionalTags { get; init; } = new();
    public string Content { get; init; } = string.Empty;
    public int SessionDurationSeconds { get; init; }
    public string EntryType { get; init; } = "spontaneous";
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
