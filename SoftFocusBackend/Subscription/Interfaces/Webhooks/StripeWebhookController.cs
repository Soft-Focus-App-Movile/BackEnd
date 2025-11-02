using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using SoftFocusBackend.Subscription.Infrastructure.ExternalServices;
using SoftFocusBackend.Subscription.Infrastructure.Repositories;

namespace SoftFocusBackend.Subscription.Interfaces.Webhooks;

/// <summary>
/// SIMPLIFIED VERSION - Configure proper webhooks after testing Stripe integration
/// </summary>
[ApiController]
[Route("api/v1/webhooks/stripe")]
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
            // Validate webhook signature (commented out for testing)
            // var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], _stripeSettings.WebhookSecret);

            _logger.LogInformation("Received Stripe webhook. Body length: {Length}", json.Length);

            // TODO: Implement webhook handlers after Stripe is fully configured
            // For now, just log and return OK

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook");
            return StatusCode(500);
        }
    }
}
