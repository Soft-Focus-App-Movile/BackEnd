namespace SoftFocusBackend.Tracking.Interfaces.REST.Resources;

public record TrackingSummaryResource
{
    public bool HasTodayCheckIn { get; init; }
    public CheckInResource? TodayCheckIn { get; init; }
    public int TotalCheckIns { get; init; }
    public int TotalEmotionalCalendarEntries { get; init; }
    public double AverageEmotionalLevel { get; init; }
    public double AverageEnergyLevel { get; init; }
    public double AverageMoodLevel { get; init; }
    public List<string> MostCommonSymptoms { get; init; } = new();
    public List<string> MostUsedEmotionalTags { get; init; } = new();
}