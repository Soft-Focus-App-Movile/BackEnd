using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SoftFocusBackend.Shared.Domain.Entities;
using SoftFocusBackend.Subscription.Domain.ValueObjects;

namespace SoftFocusBackend.Subscription.Domain.Aggregates;

/// <summary>
/// Tracks feature usage for subscription limits validation
/// </summary>
public class UsageTracking : BaseEntity
{
    [BsonElement("userId")]
    public string UserId { get; private set; }

    [BsonElement("featureType")]
    [BsonRepresentation(BsonType.String)]
    public FeatureType FeatureType { get; private set; }

    [BsonElement("usageCount")]
    public int UsageCount { get; private set; }

    [BsonElement("periodStart")]
    public DateTime PeriodStart { get; private set; }

    [BsonElement("periodEnd")]
    public DateTime PeriodEnd { get; private set; }

    [BsonElement("lastUsedAt")]
    public DateTime LastUsedAt { get; private set; }

    // For MongoDB
    private UsageTracking() { }

    public UsageTracking(string userId, FeatureType featureType, DateTime periodStart, DateTime periodEnd)
    {
        Id = ObjectId.GenerateNewId().ToString();
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        FeatureType = featureType;
        UsageCount = 0;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Increment usage count
    /// </summary>
    public void IncrementUsage()
    {
        UsageCount++;
        LastUsedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Reset usage count for new period
    /// </summary>
    public void ResetForNewPeriod(DateTime newPeriodStart, DateTime newPeriodEnd)
    {
        UsageCount = 0;
        PeriodStart = newPeriodStart;
        PeriodEnd = newPeriodEnd;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if current period has expired
    /// </summary>
    public bool IsPeriodExpired()
    {
        return DateTime.UtcNow > PeriodEnd;
    }

    /// <summary>
    /// Check if limit is reached
    /// </summary>
    public bool IsLimitReached(int? limit)
    {
        // Null limit means unlimited
        if (limit == null) return false;

        return UsageCount >= limit.Value;
    }
}
