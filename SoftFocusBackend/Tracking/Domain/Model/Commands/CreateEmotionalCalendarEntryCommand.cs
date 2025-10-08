namespace SoftFocusBackend.Tracking.Domain.Model.Commands;

public record CreateEmotionalCalendarEntryCommand
{
    public string UserId { get; init; }
    public DateTime Date { get; init; }
    public string EmotionalEmoji { get; init; }
    public int MoodLevel { get; init; }
    public List<string> EmotionalTags { get; init; }
    public DateTime RequestedAt { get; init; }

    public CreateEmotionalCalendarEntryCommand(string userId, DateTime date, string emotionalEmoji, 
        int moodLevel, List<string> emotionalTags)
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        Date = date.Date;
        EmotionalEmoji = emotionalEmoji?.Trim() ?? throw new ArgumentNullException(nameof(emotionalEmoji));
        MoodLevel = moodLevel;
        EmotionalTags = emotionalTags ?? new List<string>();
        RequestedAt = DateTime.UtcNow;
    }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(UserId) &&
               !string.IsNullOrWhiteSpace(EmotionalEmoji) &&
               MoodLevel >= 1 && MoodLevel <= 10;
    }
}