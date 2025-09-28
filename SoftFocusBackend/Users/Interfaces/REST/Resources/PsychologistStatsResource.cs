namespace SoftFocusBackend.Users.Interfaces.REST.Resources;

public record PsychologistStatsResource
{
    public int ConnectedPatientsCount { get; init; }
    public int TotalCheckInsReceived { get; init; }
    public int CrisisAlertsHandled { get; init; }
    public string AverageResponseTime { get; init; } = string.Empty;
    public bool IsAcceptingNewPatients { get; init; }
    public DateTime? LastActivityDate { get; init; }
    public DateTime JoinedDate { get; init; }
    public double? AverageRating { get; init; }
    public int TotalReviews { get; init; }
    public string ExperienceLevel { get; init; } = string.Empty;
    public DateTime StatsGeneratedAt { get; init; }
}