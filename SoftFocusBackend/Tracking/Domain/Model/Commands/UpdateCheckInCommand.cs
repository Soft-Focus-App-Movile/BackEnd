namespace SoftFocusBackend.Tracking.Domain.Model.Commands;

public record UpdateCheckInCommand
{
    public string CheckInId { get; init; }
    public int EmotionalLevel { get; init; }
    public int EnergyLevel { get; init; }
    public string MoodDescription { get; init; }
    public decimal SleepHours { get; init; }
    public List<string> Symptoms { get; init; }
    public string Notes { get; init; }
    public DateTime RequestedAt { get; init; }

    public UpdateCheckInCommand(string checkInId, int emotionalLevel, int energyLevel, 
        string moodDescription, decimal sleepHours, List<string> symptoms, string notes)
    {
        CheckInId = checkInId ?? throw new ArgumentNullException(nameof(checkInId));
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
        return !string.IsNullOrWhiteSpace(CheckInId) &&
               EmotionalLevel >= 1 && EmotionalLevel <= 10 &&
               EnergyLevel >= 1 && EnergyLevel <= 10 &&
               SleepHours >= 0 && SleepHours <= 24;
    }
}