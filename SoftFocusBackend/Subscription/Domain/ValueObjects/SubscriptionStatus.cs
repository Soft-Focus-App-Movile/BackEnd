namespace SoftFocusBackend.Subscription.Domain.ValueObjects;

/// <summary>
/// Represents the current status of a subscription
/// </summary>
public enum SubscriptionStatus
{
    /// <summary>
    /// Subscription is active and features are available
    /// </summary>
    Active = 0,

    /// <summary>
    /// Subscription has been cancelled but still active until period end
    /// </summary>
    Cancelled = 1,

    /// <summary>
    /// Subscription has expired
    /// </summary>
    Expired = 2,

    /// <summary>
    /// Payment failed, retry in progress
    /// </summary>
    PastDue = 3,

    /// <summary>
    /// Subscription is in trial period
    /// </summary>
    Trial = 4
}
