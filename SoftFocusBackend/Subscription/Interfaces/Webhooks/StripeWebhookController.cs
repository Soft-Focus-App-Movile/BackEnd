using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using SoftFocusBackend.Subscription.Infrastructure.ExternalServices;
using SoftFocusBackend.Subscription.Infrastructure.Repositories;

namespace SoftFocusBackend.Subscription.Interfaces.Webhooks;

/// <summary>
/// Handles Stripe webhook events with signature validation
/// </summary>
[ApiController]
[Route("api/v1/webhooks/stripe")]
[AllowAnonymous]
public class StripeWebhookController : ControllerBase
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly StripeSettings _stripeSettings;
    private readonly ILogger<StripeWebhookController> _logger;

    public StripeWebhookController(
        ISubscriptionRepository subscriptionRepository,
        IOptions<StripeSettings> stripeSettings,
        ILogger<StripeWebhookController> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _stripeSettings = stripeSettings.Value;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> HandleWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        try
        {
            // Validate webhook signature
            var stripeSignature = Request.Headers["Stripe-Signature"];

            if (string.IsNullOrEmpty(stripeSignature))
            {
                _logger.LogWarning("Stripe webhook received without signature header");
                return BadRequest("Missing Stripe signature");
            }

            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    stripeSignature,
                    _stripeSettings.WebhookSecret
                );
            }
            catch (StripeException ex)
            {
                _logger.LogWarning(ex, "Invalid Stripe webhook signature");
                return BadRequest("Invalid signature");
            }

            _logger.LogInformation("Received valid Stripe webhook: {EventType}, ID: {EventId}",
                stripeEvent.Type, stripeEvent.Id);

            // TODO: Implement webhook handlers for different event types
            // Common events to handle:
            // - checkout.session.completed: Payment successful
            // - customer.subscription.updated: Subscription changed
            // - customer.subscription.deleted: Subscription cancelled
            // - invoice.payment_succeeded: Recurring payment successful
            // - invoice.payment_failed: Payment failed

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook");
            return StatusCode(500);
        }
    }
}
