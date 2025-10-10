using MongoDB.Bson.Serialization.Attributes;
using SoftFocusBackend.Shared.Domain.Entities;
using SoftFocusBackend.Tracking.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Tracking.Domain.Model.Aggregates;

public class EmotionalCalendar : BaseEntity
{
    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("date")]
    public DateTime Date { get; set; }

    [BsonElement("emotionalEmoji")]
    public string EmotionalEmoji { get; set; } = string.Empty;

    [BsonElement("moodLevel")]
    public int MoodLevel { get; set; }

    [BsonElement("emotionalTags")]
    public List<string> EmotionalTags { get; set; } = new();

    public void UpdateEmotionalEntry(EmotionalEmoji emoji, MoodLevel moodLevel, EmotionalTags tags)
    {
        EmotionalEmoji = emoji;
        MoodLevel = moodLevel;
        EmotionalTags = tags;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateEmoji(EmotionalEmoji emoji)
    {
        EmotionalEmoji = emoji;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateMoodLevel(MoodLevel moodLevel)
    {
        MoodLevel = moodLevel;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateTags(EmotionalTags tags)
    {
        EmotionalTags = tags;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsFromToday()
    {
        return Date.Date == DateTime.UtcNow.Date;
    }

    public bool IsFromThisWeek()
    {
        var startOfWeek = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
        var endOfWeek = startOfWeek.AddDays(7);
        return Date.Date >= startOfWeek && Date.Date < endOfWeek;
    }

    public void ValidateForCreation()
    {
        if (string.IsNullOrWhiteSpace(UserId))
            throw new ArgumentException("User ID is required");

        if (MoodLevel < 1 || MoodLevel > 10)
            throw new ArgumentException("Mood level must be between 1 and 10");

        if (string.IsNullOrWhiteSpace(EmotionalEmoji))
            throw new ArgumentException("Emotional emoji is required");
    }
}