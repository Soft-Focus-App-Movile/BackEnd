namespace SoftFocusBackend.Subscription.Domain.ValueObjects;

/// <summary>
/// Represents the subscription plan type
/// </summary>
public enum SubscriptionPlan
{
    /// <summary>
    /// Free basic plan with limited features
    /// </summary>
    Basic = 0,

    /// <summary>
    /// Premium plan with unlimited features - $12.99/month
    /// </summary>
    Pro = 1
}
