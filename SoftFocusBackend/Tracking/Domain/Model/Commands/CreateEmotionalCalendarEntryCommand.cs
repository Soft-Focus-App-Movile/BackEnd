namespace SoftFocusBackend.Tracking.Domain.Model.Commands;

public record CreateEmotionalCalendarEntryCommand
{
    public string UserId { get; init; }
    public DateTime Timestamp { get; init; }
    public string EmotionalEmoji { get; init; }
    public int MoodLevel { get; init; }
    public List<string> EmotionalTags { get; init; }
    public string Content { get; init; }
    public int SessionDurationSeconds { get; init; }
    public string EntryType { get; init; }
    public DateTime RequestedAt { get; init; }

    public CreateEmotionalCalendarEntryCommand(string userId, DateTime timestamp, string emotionalEmoji,
        int moodLevel, List<string> emotionalTags, string content = "",
        int sessionDurationSeconds = 0, string entryType = "spontaneous")
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        Timestamp = timestamp;
        EmotionalEmoji = emotionalEmoji?.Trim() ?? throw new ArgumentNullException(nameof(emotionalEmoji));
        MoodLevel = moodLevel;
        EmotionalTags = emotionalTags ?? new List<string>();
        Content = content ?? string.Empty;
        SessionDurationSeconds = sessionDurationSeconds;
        EntryType = string.IsNullOrWhiteSpace(entryType) ? "spontaneous" : entryType.Trim().ToLowerInvariant();
        RequestedAt = DateTime.UtcNow;

        if (Timestamp > DateTime.UtcNow.AddMinutes(5))
            throw new ArgumentException("Timestamp cannot be in the future", nameof(timestamp));

        if (EntryType != "scheduled" && EntryType != "spontaneous")
            throw new ArgumentException("Entry type must be 'scheduled' or 'spontaneous'", nameof(entryType));
    }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(UserId) &&
               !string.IsNullOrWhiteSpace(EmotionalEmoji) &&
               MoodLevel >= 1 && MoodLevel <= 10 &&
               SessionDurationSeconds >= 0 &&
               Timestamp <= DateTime.UtcNow.AddMinutes(5) &&
               (EntryType == "scheduled" || EntryType == "spontaneous");
    }
}
