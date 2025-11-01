using SoftFocusBackend.Users.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Subscription.Domain.ValueObjects;

/// <summary>
/// Defines usage limits for each subscription plan and user type
/// </summary>
public class UsageLimits
{
    // AI Chat limits
    public int? AiChatMessagesPerDay { get; private set; }

    // Facial analysis limits
    public int? FacialAnalysisPerWeek { get; private set; }

    // Content recommendations limits
    public int? ContentRecommendationsPerWeek { get; private set; }

    // Check-ins limits
    public int? CheckInsPerDay { get; private set; }

    // Psychologist-specific limits
    public int? MaxPatientConnections { get; private set; }

    // Content assignment limits (for psychologists)
    public int? ContentAssignmentsPerWeek { get; private set; }

    private UsageLimits() { }

    /// <summary>
    /// Gets the usage limits for a specific plan and user type
    /// </summary>
    public static UsageLimits GetLimitsForPlan(SubscriptionPlan plan, UserType userType)
    {
        return userType switch
        {
            UserType.General => GetGeneralUserLimits(plan),
            UserType.Psychologist => GetPsychologistLimits(plan),
            _ => throw new ArgumentException($"Invalid user type: {userType}")
        };
    }

    private static UsageLimits GetGeneralUserLimits(SubscriptionPlan plan)
    {
        return plan switch
        {
            SubscriptionPlan.Basic => new UsageLimits
            {
                AiChatMessagesPerDay = 3,
                FacialAnalysisPerWeek = 2,
                ContentRecommendationsPerWeek = 10,
                CheckInsPerDay = 1,
                MaxPatientConnections = null, // N/A for general users
                ContentAssignmentsPerWeek = null // N/A for general users
            },
            SubscriptionPlan.Pro => new UsageLimits
            {
                AiChatMessagesPerDay = null, // Unlimited
                FacialAnalysisPerWeek = null, // Unlimited
                ContentRecommendationsPerWeek = null, // Unlimited
                CheckInsPerDay = null, // Unlimited
                MaxPatientConnections = null, // N/A for general users
                ContentAssignmentsPerWeek = null // N/A for general users
            },
            _ => throw new ArgumentException($"Invalid subscription plan: {plan}")
        };
    }

    private static UsageLimits GetPsychologistLimits(SubscriptionPlan plan)
    {
        return plan switch
        {
            SubscriptionPlan.Basic => new UsageLimits
            {
                AiChatMessagesPerDay = null, // N/A for psychologists
                FacialAnalysisPerWeek = null, // N/A for psychologists
                ContentRecommendationsPerWeek = null, // N/A for psychologists
                CheckInsPerDay = null, // N/A for psychologists
                MaxPatientConnections = 3,
                ContentAssignmentsPerWeek = 5
            },
            SubscriptionPlan.Pro => new UsageLimits
            {
                AiChatMessagesPerDay = null, // N/A for psychologists
                FacialAnalysisPerWeek = null, // N/A for psychologists
                ContentRecommendationsPerWeek = null, // N/A for psychologists
                CheckInsPerDay = null, // N/A for psychologists
                MaxPatientConnections = 50, // High limit for Pro
                ContentAssignmentsPerWeek = null // Unlimited
            },
            _ => throw new ArgumentException($"Invalid subscription plan: {plan}")
        };
    }

    /// <summary>
    /// Check if a specific feature has unlimited access
    /// </summary>
    public bool IsUnlimited(string featureName)
    {
        return featureName switch
        {
            "AiChatMessages" => AiChatMessagesPerDay == null,
            "FacialAnalysis" => FacialAnalysisPerWeek == null,
            "ContentRecommendations" => ContentRecommendationsPerWeek == null,
            "CheckIns" => CheckInsPerDay == null,
            "PatientConnections" => MaxPatientConnections == null,
            "ContentAssignments" => ContentAssignmentsPerWeek == null,
            _ => false
        };
    }
}
