using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;
using SoftFocusBackend.Subscription.Infrastructure.ExternalServices;
using SoftFocusBackend.Subscription.Infrastructure.Repositories;
using DomainSubscription = SoftFocusBackend.Subscription.Domain.Aggregates.Subscription;
using StripeSubscription = Stripe.Subscription;

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

    /// <summary>
    /// Stripe webhook endpoint
    /// </summary>
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

            // Handle the event
            switch (stripeEvent.Type)
            {
                case "checkout.session.completed":
                    await HandleCheckoutSessionCompleted(stripeEvent);
                    break;

                case "customer.subscription.updated":
                    await HandleSubscriptionUpdated(stripeEvent);
                    break;

                case "customer.subscription.deleted":
                    await HandleSubscriptionDeleted(stripeEvent);
                    break;

                case "invoice.payment_succeeded":
                    await HandleInvoicePaymentSucceeded(stripeEvent);
                    break;

                case "invoice.payment_failed":
                    await HandleInvoicePaymentFailed(stripeEvent);
                    break;

                default:
                    _logger.LogInformation("Unhandled webhook event type: {EventType}", stripeEvent.Type);
                    break;
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook");
            return StatusCode(500);
        }
    }

    private async Task HandleCheckoutSessionCompleted(Event stripeEvent)
    {
        var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
        if (session == null) return;

        _logger.LogInformation("Checkout session completed: {SessionId}", session.Id);

        try
        {
            // The subscription upgrade will be handled by the success URL callback
            // This is just for logging and verification
            var subscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(
                session.SubscriptionId);

            if (subscription != null)
            {
                _logger.LogInformation("Subscription found for checkout session: {SubscriptionId}",
                    subscription.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling checkout session completed");
        }
    }

    private async Task HandleSubscriptionUpdated(Event stripeEvent)
    {
        var stripeSubscription = stripeEvent.Data.Object as StripeSubscription;
        if (stripeSubscription == null) return;

        _logger.LogInformation("Subscription updated: {SubscriptionId}, Status: {Status}",
            stripeSubscription.Id, stripeSubscription.Status);

        try
        {
            var subscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(
                stripeSubscription.Id);

            if (subscription == null)
            {
                _logger.LogWarning("Subscription not found for Stripe subscription: {SubscriptionId}",
                    stripeSubscription.Id);
                return;
            }

            // Update period dates from subscription items (v48+ moved these to SubscriptionItem)
            if (stripeSubscription.Items?.Data != null && stripeSubscription.Items.Data.Any())
            {
                // Get the latest period dates from subscription items
                var firstItem = stripeSubscription.Items.Data.First();
                var periodStart = firstItem.CurrentPeriodStart;
                var periodEnd = firstItem.CurrentPeriodEnd;

                if (subscription.CurrentPeriodStart != periodStart || subscription.CurrentPeriodEnd != periodEnd)
                {
                    subscription.RenewSubscription(periodStart, periodEnd);
                    await _subscriptionRepository.UpdateAsync(subscription);

                    _logger.LogInformation("Updated subscription period for: {SubscriptionId}", subscription.Id);
                }
            }

            // Handle cancellation at period end
            if (stripeSubscription.CancelAtPeriodEnd && !subscription.CancelAtPeriodEnd)
            {
                subscription.CancelAtEndOfPeriod();
                await _subscriptionRepository.UpdateAsync(subscription);

                _logger.LogInformation("Marked subscription for cancellation: {SubscriptionId}",
                    subscription.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling subscription updated");
        }
    }

    private async Task HandleSubscriptionDeleted(Event stripeEvent)
    {
        var stripeSubscription = stripeEvent.Data.Object as StripeSubscription;
        if (stripeSubscription == null) return;

        _logger.LogInformation("Subscription deleted: {SubscriptionId}", stripeSubscription.Id);

        try
        {
            var subscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(
                stripeSubscription.Id);

            if (subscription == null)
            {
                _logger.LogWarning("Subscription not found for deleted Stripe subscription: {SubscriptionId}",
                    stripeSubscription.Id);
                return;
            }

            // Downgrade to Basic
            subscription.DowngradeToBasic();
            await _subscriptionRepository.UpdateAsync(subscription);

            _logger.LogInformation("Downgraded subscription to Basic: {SubscriptionId}",
                subscription.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling subscription deleted");
        }
    }

    private async Task HandleInvoicePaymentSucceeded(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Invoice;
        if (invoice == null) return;

        // Extract subscription ID from lines (subscription invoices always have subscription items)
        var subscriptionId = invoice.Lines?.Data?.FirstOrDefault()?.SubscriptionId;

        _logger.LogInformation("Invoice payment succeeded: {InvoiceId}, Subscription: {SubscriptionId}",
            invoice.Id, subscriptionId);

        try
        {
            if (string.IsNullOrEmpty(subscriptionId)) return;

            var subscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(subscriptionId);

            if (subscription == null)
            {
                _logger.LogWarning("Subscription not found for invoice: {InvoiceId}", invoice.Id);
                return;
            }

            // If subscription was past due, mark as active again
            if (subscription.Status == Domain.ValueObjects.SubscriptionStatus.PastDue)
            {
                subscription.RenewSubscription(
                    DateTime.UtcNow,
                    DateTime.UtcNow.AddMonths(1));

                await _subscriptionRepository.UpdateAsync(subscription);

                _logger.LogInformation("Reactivated subscription after payment: {SubscriptionId}",
                    subscription.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling invoice payment succeeded");
        }
    }

    private async Task HandleInvoicePaymentFailed(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Invoice;
        if (invoice == null) return;

        // Extract subscription ID from lines (subscription invoices always have subscription items)
        var subscriptionId = invoice.Lines?.Data?.FirstOrDefault()?.SubscriptionId;

        _logger.LogWarning("Invoice payment failed: {InvoiceId}, Subscription: {SubscriptionId}",
            invoice.Id, subscriptionId);

        try
        {
            if (string.IsNullOrEmpty(subscriptionId)) return;

            var subscription = await _subscriptionRepository.GetByStripeSubscriptionIdAsync(subscriptionId);

            if (subscription == null)
            {
                _logger.LogWarning("Subscription not found for failed invoice: {InvoiceId}",
                    invoice.Id);
                return;
            }

            // Mark subscription as past due
            subscription.MarkAsPastDue();
            await _subscriptionRepository.UpdateAsync(subscription);

            _logger.LogInformation("Marked subscription as past due: {SubscriptionId}",
                subscription.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling invoice payment failed");
        }
    }
}
