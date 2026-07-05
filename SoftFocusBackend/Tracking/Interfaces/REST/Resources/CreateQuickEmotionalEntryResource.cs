using System.ComponentModel.DataAnnotations;

namespace SoftFocusBackend.Tracking.Interfaces.REST.Resources;

public record CreateQuickEmotionalEntryResource
{
    [Required]
    public DateTime Timestamp { get; init; }

    [Required]
    [StringLength(10, ErrorMessage = "Emotional emoji cannot exceed 10 characters")]
    public string EmotionalEmoji { get; init; } = string.Empty;

    [Required]
    [Range(1, 10, ErrorMessage = "Mood level must be between 1 and 10")]
    public int MoodLevel { get; init; }

    [StringLength(2000, ErrorMessage = "Content cannot exceed 2000 characters")]
    public string Content { get; init; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "SessionDurationSeconds must be >= 0")]
    public int SessionDurationSeconds { get; init; } = 0;
}
