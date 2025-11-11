namespace SoftFocusBackend.Users.Domain.Model.ValueObjects;

public record PsychologistStats
{
    public int ActivePatientsCount { get; init; }
    public int PendingCrisisAlerts { get; init; }
    public int TodayCheckInsCompleted { get; init; }
    public double AverageAdherenceRate { get; init; }
    public int NewPatientsThisMonth { get; init; }
    public double AverageEmotionalLevel { get; init; }

    public PsychologistStats(
        int activePatientsCount,
        int pendingCrisisAlerts,
        int todayCheckInsCompleted,
        double averageAdherenceRate,
        int newPatientsThisMonth,
        double averageEmotionalLevel)
    {
        if (activePatientsCount < 0)
            throw new ArgumentException("Active patients count cannot be negative.", nameof(activePatientsCount));

        if (pendingCrisisAlerts < 0)
            throw new ArgumentException("Pending crisis alerts cannot be negative.", nameof(pendingCrisisAlerts));

        if (todayCheckInsCompleted < 0)
            throw new ArgumentException("Today check-ins completed cannot be negative.", nameof(todayCheckInsCompleted));

        if (averageAdherenceRate < 0 || averageAdherenceRate > 100)
            throw new ArgumentException("Average adherence rate must be between 0 and 100.", nameof(averageAdherenceRate));

        if (newPatientsThisMonth < 0)
            throw new ArgumentException("New patients this month cannot be negative.", nameof(newPatientsThisMonth));

        if (averageEmotionalLevel < 0 || averageEmotionalLevel > 10)
            throw new ArgumentException("Average emotional level must be between 0 and 10.", nameof(averageEmotionalLevel));

        ActivePatientsCount = activePatientsCount;
        PendingCrisisAlerts = pendingCrisisAlerts;
        TodayCheckInsCompleted = todayCheckInsCompleted;
        AverageAdherenceRate = averageAdherenceRate;
        NewPatientsThisMonth = newPatientsThisMonth;
        AverageEmotionalLevel = averageEmotionalLevel;
    }
}