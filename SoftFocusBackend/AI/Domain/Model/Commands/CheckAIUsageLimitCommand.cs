namespace SoftFocusBackend.AI.Domain.Model.Commands;

public record CheckAIUsageLimitCommand
{
    public string UserId { get; init; }
    public AIFeatureType FeatureType { get; init; }

    public CheckAIUsageLimitCommand(string userId, AIFeatureType featureType)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required", nameof(userId));

        UserId = userId;
        FeatureType = featureType;
    }

    public string GetAuditString()
    {
        return $"Checking AI usage limit for User {UserId}, Feature: {FeatureType}";
    }
}

public enum AIFeatureType
{
    Chat,
    FacialAnalysis
}
