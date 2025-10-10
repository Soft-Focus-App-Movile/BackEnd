using System.ComponentModel.DataAnnotations;

namespace SoftFocusBackend.Tracking.Interfaces.REST.Resources;

public record UpdateEmotionalCalendarEntryResource
{
    [Required]
    [StringLength(10, ErrorMessage = "Emotional emoji cannot exceed 10 characters")]
    public string EmotionalEmoji { get; init; } = string.Empty;

    [Required]
    [Range(1, 10, ErrorMessage = "Mood level must be between 1 and 10")]
    public int MoodLevel { get; init; }

    public List<string> EmotionalTags { get; init; } = new();
}