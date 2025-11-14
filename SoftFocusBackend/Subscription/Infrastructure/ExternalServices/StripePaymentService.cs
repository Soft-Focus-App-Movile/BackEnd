using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace SoftFocusBackend.Subscription.Infrastructure.ExternalServices;

public class StripePaymentService : IStripePaymentService
{
    private readonly StripeSettings _settings;
    private readonly ILogger<StripePaymentService> _logger;

    public StripePaymentService(
        IOptions<StripeSettings> settings,
        ILogger<StripePaymentService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        // Set Stripe API Key
        StripeConfiguration.ApiKey = _settings.SecretKey;
    }

    public async Task<string> CreateCustomerAsync(string email, string name)
    {
        try
        {
            var options = new CustomerCreateOptions
            {
                Email = email,
                Name = name,
                Metadata = new Dictionary<string, string>
                {
                    { "source", "SoftFocus" }
                }
            };

            var service = new CustomerService();
            var customer = await service.CreateAsync(options);

            _logger.LogInformation("Created Stripe customer: {CustomerId} for email: {Email}",
                customer.Id, email);

            return customer.Id;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error creating Stripe customer for email: {Email}", email);
            throw new InvalidOperationException("Failed to create payment customer", ex);
        }
    }

    public async Task<Session> CreateCheckoutSessionAsync(
        string customerId,
        string successUrl,
        string cancelUrl)
    {
        try
        {
            var options = new SessionCreateOptions
            {
                Customer = customerId,
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Price = _settings.ProPriceId,
                        Quantity = 1
                    }
                },
                Mode = "subscription",
                // CRITICAL FIX: Add {CHECKOUT_SESSION_ID} placeholder so Stripe includes session_id in redirect
                SuccessUrl = successUrl + "?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = cancelUrl,
                BillingAddressCollection = "auto",
                AllowPromotionCodes = true
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            _logger.LogInformation("Created checkout session: {SessionId} for customer: {CustomerId}",
                session.Id, customerId);

            return session;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error creating checkout session for customer: {CustomerId}", customerId);
            throw new InvalidOperationException("Failed to create checkout session", ex);
        }
    }

    public async Task<string> CreatePortalSessionAsync(string customerId, string returnUrl)
    {
        try
        {
            var options = new Stripe.BillingPortal.SessionCreateOptions
            {
                Customer = customerId,
                ReturnUrl = returnUrl
            };

            var service = new Stripe.BillingPortal.SessionService();
            var session = await service.CreateAsync(options);

            _logger.LogInformation("Created portal session for customer: {CustomerId}", customerId);

            return session.Url;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error creating portal session for customer: {CustomerId}", customerId);
            throw new InvalidOperationException("Failed to create portal session", ex);
        }
    }

    public async Task CancelSubscriptionAsync(string subscriptionId)
    {
        try
        {
            var service = new SubscriptionService();
            var options = new SubscriptionUpdateOptions
            {
                CancelAtPeriodEnd = true
            };

            await service.UpdateAsync(subscriptionId, options);

            _logger.LogInformation("Marked subscription for cancellation: {SubscriptionId}", subscriptionId);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error cancelling subscription: {SubscriptionId}", subscriptionId);
            throw new InvalidOperationException("Failed to cancel subscription", ex);
        }
    }

    public async Task CancelSubscriptionImmediatelyAsync(string subscriptionId)
    {
        try
        {
            var service = new SubscriptionService();
            await service.CancelAsync(subscriptionId);

            _logger.LogInformation("Cancelled subscription immediately: {SubscriptionId}", subscriptionId);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error cancelling subscription immediately: {SubscriptionId}", subscriptionId);
            throw new InvalidOperationException("Failed to cancel subscription immediately", ex);
        }
    }

    public async Task<Stripe.Subscription> GetSubscriptionAsync(string subscriptionId)
    {
        try
        {
            var service = new SubscriptionService();
            return await service.GetAsync(subscriptionId);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error getting subscription: {SubscriptionId}", subscriptionId);
            throw new InvalidOperationException("Failed to get subscription details", ex);
        }
    }

    public async Task<string> CreateProPriceAsync()
    {
        try
        {
            // First, create a product
            var productOptions = new ProductCreateOptions
            {
                Name = "SoftFocus Pro",
                Description = "Premium subscription with unlimited features for mental health tracking",
            };

            var productService = new ProductService();
            var product = await productService.CreateAsync(productOptions);

            // Then, create a price for the product
            var priceOptions = new PriceCreateOptions
            {
                Product = product.Id,
                UnitAmount = 1299, // $12.99 in cents
                Currency = "usd",
                Recurring = new PriceRecurringOptions
                {
                    Interval = "month"
                }
            };

            var priceService = new PriceService();
            var price = await priceService.CreateAsync(priceOptions);

            _logger.LogInformation("Created Stripe price: {PriceId} for product: {ProductId}",
                price.Id, product.Id);

            return price.Id;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Error creating Stripe price");
            throw new InvalidOperationException("Failed to create price", ex);
        }
    }
}

public class StripeSettings
{
    public string PublishableKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string ProPriceId { get; set; } = string.Empty;
}
