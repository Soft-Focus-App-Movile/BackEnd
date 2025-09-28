namespace SoftFocusBackend.Users.Domain.Model.ValueObjects;

public record PsychologistStats
{
    public int ConnectedPatientsCount { get; init; }
    public int TotalCheckInsReceived { get; init; }
    public int CrisisAlertsHandled { get; init; }
    public TimeSpan AverageResponseTime { get; init; }
    public bool IsAcceptingNewPatients { get; init; }
    public DateTime? LastActivityDate { get; init; }
    public DateTime JoinedDate { get; init; }
    public double? AverageRating { get; init; }
    public int TotalReviews { get; init; }

    public PsychologistStats(int connectedPatientsCount, int totalCheckInsReceived, 
        int crisisAlertsHandled, TimeSpan averageResponseTime, bool isAcceptingNewPatients,
        DateTime? lastActivityDate, DateTime joinedDate, double? averageRating = null, 
        int totalReviews = 0)
    {
        if (connectedPatientsCount < 0)
            throw new ArgumentException("Connected patients count cannot be negative.", nameof(connectedPatientsCount));
        
        if (totalCheckInsReceived < 0)
            throw new ArgumentException("Total check-ins received cannot be negative.", nameof(totalCheckInsReceived));
        
        if (crisisAlertsHandled < 0)
            throw new ArgumentException("Crisis alerts handled cannot be negative.", nameof(crisisAlertsHandled));

        if (averageRating.HasValue && (averageRating < 0 || averageRating > 5))
            throw new ArgumentException("Average rating must be between 0 and 5.", nameof(averageRating));

        if (totalReviews < 0)
            throw new ArgumentException("Total reviews cannot be negative.", nameof(totalReviews));

        ConnectedPatientsCount = connectedPatientsCount;
        TotalCheckInsReceived = totalCheckInsReceived;
        CrisisAlertsHandled = crisisAlertsHandled;
        AverageResponseTime = averageResponseTime;
        IsAcceptingNewPatients = isAcceptingNewPatients;
        LastActivityDate = lastActivityDate;
        JoinedDate = joinedDate;
        AverageRating = averageRating;
        TotalReviews = totalReviews;
    }

    public string GetFormattedResponseTime()
    {
        if (AverageResponseTime.TotalMinutes < 1)
            return "< 1 minute";
        
        if (AverageResponseTime.TotalMinutes < 60)
            return $"{AverageResponseTime.TotalMinutes:F0} minutes";
        
        return $"{AverageResponseTime.TotalHours:F1} hours";
    }

    public string GetExperienceLevel()
    {
        var experience = DateTime.UtcNow - JoinedDate;
        
        if (experience.TotalDays < 30)
            return "New";
        
        if (experience.TotalDays < 365)
            return "Experienced";
        
        return "Veteran";
    }
}