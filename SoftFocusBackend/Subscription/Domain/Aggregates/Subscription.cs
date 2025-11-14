using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SoftFocusBackend.Shared.Domain.Entities;
using SoftFocusBackend.Subscription.Domain.ValueObjects;
using SoftFocusBackend.Users.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Subscription.Domain.Aggregates;

/// <summary>
/// Represents a user's subscription to the platform
/// </summary>
public class Subscription : BaseEntity
{
    [BsonElement("userId")]
    public string UserId { get; private set; }

    [BsonElement("userType")]
    [BsonRepresentation(BsonType.String)]
    public UserType UserType { get; private set; }

    [BsonElement("plan")]
    [BsonRepresentation(BsonType.String)]
    public SubscriptionPlan Plan { get; private set; }

    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    public SubscriptionStatus Status { get; private set; }

    [BsonElement("stripeCustomerId")]
    public string? StripeCustomerId { get; private set; }

    [BsonElement("stripeSubscriptionId")]
    public string? StripeSubscriptionId { get; private set; }

    [BsonElement("currentPeriodStart")]
    public DateTime? CurrentPeriodStart { get; private set; }

    [BsonElement("currentPeriodEnd")]
    public DateTime? CurrentPeriodEnd { get; private set; }

    [BsonElement("cancelAtPeriodEnd")]
    public bool CancelAtPeriodEnd { get; private set; }

    [BsonElement("cancelledAt")]
    public DateTime? CancelledAt { get; private set; }

    [BsonElement("trialStart")]
    public DateTime? TrialStart { get; private set; }

    [BsonElement("trialEnd")]
    public DateTime? TrialEnd { get; private set; }

    // For MongoDB
    private Subscription() { }

    /// <summary>
    /// Creates a new Basic (free) subscription
    /// </summary>
    public static Subscription CreateBasicSubscription(string userId, UserType userType)
    {
        return new Subscription
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = userId ?? throw new ArgumentNullException(nameof(userId)),
            UserType = userType,
            Plan = SubscriptionPlan.Basic,
            Status = SubscriptionStatus.Active,
            CancelAtPeriodEnd = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a new Pro subscription
    /// </summary>
    public static Subscription CreateProSubscription(
        string userId,
        UserType userType,
        string stripeCustomerId,
        string stripeSubscriptionId,
        DateTime periodStart,
        DateTime periodEnd)
    {
        return new Subscription
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = userId ?? throw new ArgumentNullException(nameof(userId)),
            UserType = userType,
            Plan = SubscriptionPlan.Pro,
            Status = SubscriptionStatus.Active,
            StripeCustomerId = stripeCustomerId ?? throw new ArgumentNullException(nameof(stripeCustomerId)),
            StripeSubscriptionId = stripeSubscriptionId ?? throw new ArgumentNullException(nameof(stripeSubscriptionId)),
            CurrentPeriodStart = periodStart,
            CurrentPeriodEnd = periodEnd,
            CancelAtPeriodEnd = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Upgrade from Basic to Pro
    /// </summary>
    public void UpgradeToPro(
        string stripeCustomerId,
        string stripeSubscriptionId,
        DateTime periodStart,
        DateTime periodEnd)
    {
        if (Plan == SubscriptionPlan.Pro)
            throw new InvalidOperationException("Already on Pro plan");

        Plan = SubscriptionPlan.Pro;
        Status = SubscriptionStatus.Active;
        StripeCustomerId = stripeCustomerId;
        StripeSubscriptionId = stripeSubscriptionId;
        CurrentPeriodStart = periodStart;
        CurrentPeriodEnd = periodEnd;
        CancelAtPeriodEnd = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Downgrade from Pro to Basic
    /// </summary>
    public void DowngradeToBasic()
    {
        if (Plan == SubscriptionPlan.Basic)
            throw new InvalidOperationException("Already on Basic plan");

        Plan = SubscriptionPlan.Basic;
        Status = SubscriptionStatus.Active;
        StripeCustomerId = null;
        StripeSubscriptionId = null;
        CurrentPeriodStart = null;
        CurrentPeriodEnd = null;
        CancelAtPeriodEnd = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Associate Stripe Customer ID with subscription
    /// </summary>
    public void AssociateStripeCustomer(string stripeCustomerId)
    {
        if (string.IsNullOrEmpty(stripeCustomerId))
            throw new ArgumentNullException(nameof(stripeCustomerId));

        StripeCustomerId = stripeCustomerId;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark subscription for cancellation at period end
    /// </summary>
    public void CancelAtEndOfPeriod()
    {
        if (Plan == SubscriptionPlan.Basic)
            throw new InvalidOperationException("Cannot cancel Basic plan");

        if (Status != SubscriptionStatus.Active)
            throw new InvalidOperationException("Can only cancel active subscriptions");

        CancelAtPeriodEnd = true;
        CancelledAt = DateTime.UtcNow;
        Status = SubscriptionStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Immediately cancel and downgrade subscription
    /// </summary>
    public void CancelImmediately()
    {
        if (Plan == SubscriptionPlan.Basic)
            throw new InvalidOperationException("Cannot cancel Basic plan");

        Plan = SubscriptionPlan.Basic;
        Status = SubscriptionStatus.Active;
        StripeCustomerId = null;
        StripeSubscriptionId = null;
        CurrentPeriodStart = null;
        CurrentPeriodEnd = null;
        CancelAtPeriodEnd = false;
        CancelledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Renew Pro subscription for next period
    /// </summary>
    public void RenewSubscription(DateTime newPeriodStart, DateTime newPeriodEnd)
    {
        if (Plan != SubscriptionPlan.Pro)
            throw new InvalidOperationException("Only Pro subscriptions can be renewed");

        CurrentPeriodStart = newPeriodStart;
        CurrentPeriodEnd = newPeriodEnd;
        Status = SubscriptionStatus.Active;
        CancelAtPeriodEnd = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark subscription as past due (payment failed)
    /// </summary>
    public void MarkAsPastDue()
    {
        if (Plan != SubscriptionPlan.Pro)
            throw new InvalidOperationException("Only Pro subscriptions can be past due");

        Status = SubscriptionStatus.PastDue;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark subscription as expired
    /// </summary>
    public void MarkAsExpired()
    {
        Status = SubscriptionStatus.Expired;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Start trial period
    /// </summary>
    public void StartTrial(int trialDays)
    {
        if (Plan != SubscriptionPlan.Pro)
            throw new InvalidOperationException("Only Pro subscriptions can have trials");

        TrialStart = DateTime.UtcNow;
        TrialEnd = DateTime.UtcNow.AddDays(trialDays);
        Status = SubscriptionStatus.Trial;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// End trial and activate subscription
    /// </summary>
    public void EndTrial()
    {
        if (Status != SubscriptionStatus.Trial)
            throw new InvalidOperationException("Not in trial period");

        Status = SubscriptionStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if subscription is currently active
    /// </summary>
    public bool IsActive()
    {
        return Status == SubscriptionStatus.Active || Status == SubscriptionStatus.Trial;
    }

    /// <summary>
    /// Check if subscription has expired
    /// </summary>
    public bool HasExpired()
    {
        if (Plan == SubscriptionPlan.Basic) return false;

        return CurrentPeriodEnd.HasValue && DateTime.UtcNow > CurrentPeriodEnd.Value;
    }

    /// <summary>
    /// Get usage limits for current subscription
    /// </summary>
    public UsageLimits GetUsageLimits()
    {
        return UsageLimits.GetLimitsForPlan(Plan, UserType);
    }
}
