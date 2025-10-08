using System.ComponentModel.DataAnnotations;

namespace SoftFocusBackend.Tracking.Interfaces.REST.Resources;

public record CreateCheckInResource
{
    [Required]
    [Range(1, 10, ErrorMessage = "Emotional level must be between 1 and 10")]
    public int EmotionalLevel { get; init; }

    [Required]
    [Range(1, 10, ErrorMessage = "Energy level must be between 1 and 10")]
    public int EnergyLevel { get; init; }

    [Required]
    [StringLength(500, ErrorMessage = "Mood description cannot exceed 500 characters")]
    public string MoodDescription { get; init; } = string.Empty;

    [Required]
    [Range(0, 24, ErrorMessage = "Sleep hours must be between 0 and 24")]
    public decimal SleepHours { get; init; }

    public List<string> Symptoms { get; init; } = new();

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string Notes { get; init; } = string.Empty;
}