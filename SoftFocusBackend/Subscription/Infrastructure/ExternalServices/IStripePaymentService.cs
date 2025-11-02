using Stripe.Checkout;

namespace SoftFocusBackend.Subscription.Infrastructure.ExternalServices;

public interface IStripePaymentService
{
    /// <summary>
    /// Create a Stripe Customer
    /// </summary>
    Task<string> CreateCustomerAsync(string email, string name);

    /// <summary>
    /// Create a Stripe Checkout Session for Pro subscription
    /// </summary>
    Task<Session> CreateCheckoutSessionAsync(
        string customerId,
        string successUrl,
        string cancelUrl);

    /// <summary>
    /// Create a Customer Portal Session for managing subscription
    /// </summary>
    Task<string> CreatePortalSessionAsync(string customerId, string returnUrl);

    /// <summary>
    /// Cancel a subscription at period end
    /// </summary>
    Task CancelSubscriptionAsync(string subscriptionId);

    /// <summary>
    /// Cancel a subscription immediately
    /// </summary>
    Task CancelSubscriptionImmediatelyAsync(string subscriptionId);

    /// <summary>
    /// Get subscription details from Stripe
    /// </summary>
    Task<Stripe.Subscription> GetSubscriptionAsync(string subscriptionId);

    /// <summary>
    /// Create a Price for the Pro plan (one-time setup)
    /// </summary>
    Task<string> CreateProPriceAsync();
}
