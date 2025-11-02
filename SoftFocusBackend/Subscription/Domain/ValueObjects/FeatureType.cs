namespace SoftFocusBackend.Subscription.Domain.ValueObjects;

/// <summary>
/// Represents different features that can be limited by subscription
/// </summary>
public enum FeatureType
{
    AiChatMessage,
    FacialAnalysis,
    ContentRecommendation,
    CheckIn,
    PatientConnection,
    ContentAssignment
}
