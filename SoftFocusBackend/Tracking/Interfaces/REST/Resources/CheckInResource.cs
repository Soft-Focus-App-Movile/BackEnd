namespace SoftFocusBackend.Tracking.Interfaces.REST.Resources;

public record CheckInResource
{
    public string Id { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public int EmotionalLevel { get; init; }
    public int EnergyLevel { get; init; }
    public string MoodDescription { get; init; } = string.Empty;
    public decimal SleepHours { get; init; }
    public List<string> Symptoms { get; init; } = new();
    public string Notes { get; init; } = string.Empty;
    public DateTime CompletedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}