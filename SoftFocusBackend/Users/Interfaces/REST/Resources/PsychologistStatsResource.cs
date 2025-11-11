namespace SoftFocusBackend.Users.Interfaces.REST.Resources;

public record PsychologistStatsResource
{
    // Estadísticas principales para el dashboard
    public int ActivePatientsCount { get; init; }
    public int PendingCrisisAlerts { get; init; }
    public int TodayCheckInsCompleted { get; init; }
    public double AverageAdherenceRate { get; init; }
    public int NewPatientsThisMonth { get; init; }
    public double AverageEmotionalLevel { get; init; }

    // Información adicional
    public DateTime StatsGeneratedAt { get; init; }
}