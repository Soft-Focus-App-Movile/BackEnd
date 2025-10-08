namespace SoftFocusBackend.Tracking.Domain.Model.Commands;

public record UpdateEmotionalCalendarEntryCommand
{
    public string EntryId { get; init; }
    public string EmotionalEmoji { get; init; }
    public int MoodLevel { get; init; }
    public List<string> EmotionalTags { get; init; }
    public DateTime RequestedAt { get; init; }

    public UpdateEmotionalCalendarEntryCommand(string entryId, string emotionalEmoji, 
        int moodLevel, List<string> emotionalTags)
    {
        EntryId = entryId ?? throw new ArgumentNullException(nameof(entryId));
        EmotionalEmoji = emotionalEmoji?.Trim() ?? throw new ArgumentNullException(nameof(emotionalEmoji));
        MoodLevel = moodLevel;
        EmotionalTags = emotionalTags ?? new List<string>();
        RequestedAt = DateTime.UtcNow;
    }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(EntryId) &&
               !string.IsNullOrWhiteSpace(EmotionalEmoji) &&
               MoodLevel >= 1 && MoodLevel <= 10;
    }
}