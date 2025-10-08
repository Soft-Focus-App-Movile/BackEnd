namespace SoftFocusBackend.Tracking.Domain.Model.Commands;

public record CreateCheckInCommand
{
    public string UserId { get; init; }
    public int EmotionalLevel { get; init; }
    public int EnergyLevel { get; init; }
    public string MoodDescription { get; init; }
    public decimal SleepHours { get; init; }
    public List<string> Symptoms { get; init; }
    public string Notes { get; init; }
    public DateTime RequestedAt { get; init; }

    public CreateCheckInCommand(string userId, int emotionalLevel, int energyLevel, 
        string moodDescription, decimal sleepHours, List<string> symptoms, string notes)
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        EmotionalLevel = emotionalLevel;
        EnergyLevel = energyLevel;
        MoodDescription = moodDescription?.Trim() ?? string.Empty;
        SleepHours = sleepHours;
        Symptoms = symptoms ?? new List<string>();
        Notes = notes?.Trim() ?? string.Empty;
        RequestedAt = DateTime.UtcNow;
    }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(UserId) &&
               EmotionalLevel >= 1 && EmotionalLevel <= 10 &&
               EnergyLevel >= 1 && EnergyLevel <= 10 &&
               SleepHours >= 0 && SleepHours <= 24;
    }
}