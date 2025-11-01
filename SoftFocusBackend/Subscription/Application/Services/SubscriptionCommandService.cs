using SoftFocusBackend.Subscription.Application.Commands;
using SoftFocusBackend.Subscription.Application.DTOs;
using SoftFocusBackend.Subscription.Domain.Aggregates;
using SoftFocusBackend.Subscription.Domain.ValueObjects;
using SoftFocusBackend.Subscription.Infrastructure.ExternalServices;
using SoftFocusBackend.Subscription.Infrastructure.Repositories;
using SoftFocusBackend.Users.Infrastructure.Persistence.MongoDB.Repositories;
using Stripe.Checkout;
using StripeSubscription = Stripe.Subscription;

namespace SoftFocusBackend.Subscription.Application.Services;

public class SubscriptionCommandService : ISubscriptionCommandService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUsageTrackingRepository _usageTrackingRepository;
    private readonly IStripePaymentService _stripePaymentService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<SubscriptionCommandService> _logger;

    public SubscriptionCommandService(
        ISubscriptionRepository subscriptionRepository,
        IUsageTrackingRepository usageTrackingRepository,
        IStripePaymentService stripePaymentService,
        IUserRepository userRepository,
        ILogger<SubscriptionCommandService> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _usageTrackingRepository = usageTrackingRepository;
        _stripePaymentService = stripePaymentService;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<SubscriptionDto> CreateBasicSubscriptionAsync(CreateBasicSubscriptionCommand command)
    {
        // Check if subscription already exists
        var existing = await _subscriptionRepository.GetByUserIdAsync(command.UserId);
        if (existing != null)
        {
            _logger.LogWarning("Subscription already exists for user: {UserId}", command.UserId);
            return MapToDto(existing);
        }

        var subscription = Domain.Aggregates.Subscription.CreateBasicSubscription(
            command.UserId,
            command.UserType);

        await _subscriptionRepository.CreateAsync(subscription);

        _logger.LogInformation("Created Basic subscription for user: {UserId}", command.UserId);

        return MapToDto(subscription);
    }

    public async Task<CheckoutSessionResponse> CreateProCheckoutSessionAsync(
        string userId,
        CreateCheckoutSessionRequest request)
    {
        var subscription = await _subscriptionRepository.GetByUserIdAsync(userId)
            ?? throw new InvalidOperationException("User does not have a subscription");

        var user = await _userRepository.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("User not found");

        // Create or get Stripe customer
        string customerId;
        if (string.IsNullOrEmpty(subscription.StripeCustomerId))
        {
            customerId = await _stripePaymentService.CreateCustomerAsync(
                user.Email,
                user.FullName);
        }
        else
        {
            customerId = subscription.StripeCustomerId;
        }

        // Create checkout session
        var session = await _stripePaymentService.CreateCheckoutSessionAsync(
            customerId,
            request.SuccessUrl,
            request.CancelUrl);

        _logger.LogInformation("Created checkout session: {SessionId} for user: {UserId}",
            session.Id, userId);

        return new CheckoutSessionResponse
        {
            SessionId = session.Id,
            CheckoutUrl = session.Url
        };
    }

    public async Task<SubscriptionDto> HandleSuccessfulCheckoutAsync(string sessionId)
    {
        // Get session from Stripe to get subscription details
        var sessionService = new SessionService();
        var session = await sessionService.GetAsync(sessionId,
            new SessionGetOptions { Expand = new List<string> { "subscription" } });

        if (session.Subscription == null)
        {
            throw new InvalidOperationException("No subscription found in checkout session");
        }

        var stripeSubscription = session.Subscription as StripeSubscription;

        // Find subscription by customer ID
        var subscriptions = await _subscriptionRepository.GetAllAsync();
        var subscription = subscriptions.FirstOrDefault(s => s.StripeCustomerId == session.Customer.Id)
            ?? throw new InvalidOperationException("Subscription not found for customer");

        // Upgrade to Pro
        // Use current time and monthly period (Stripe will send accurate dates via webhook)
        var periodStart = DateTime.UtcNow;
        var periodEnd = DateTime.UtcNow.AddMonths(1);

        subscription.UpgradeToPro(
            session.Customer.Id,
            stripeSubscription.Id,
            periodStart,
            periodEnd);

        await _subscriptionRepository.UpdateAsync(subscription);

        _logger.LogInformation("Upgraded subscription to Pro for user: {UserId}", subscription.UserId);

        return MapToDto(subscription);
    }

    public async Task<SubscriptionDto> CancelSubscriptionAsync(CancelSubscriptionCommand command)
    {
        var subscription = await _subscriptionRepository.GetByUserIdAsync(command.UserId)
            ?? throw new InvalidOperationException("Subscription not found");

        if (subscription.Plan == SubscriptionPlan.Basic)
        {
            throw new InvalidOperationException("Cannot cancel Basic plan");
        }

        if (command.CancelImmediately)
        {
            // Cancel immediately in Stripe
            if (!string.IsNullOrEmpty(subscription.StripeSubscriptionId))
            {
                await _stripePaymentService.CancelSubscriptionImmediatelyAsync(subscription.StripeSubscriptionId);
            }

            subscription.CancelImmediately();
            _logger.LogInformation("Cancelled subscription immediately for user: {UserId}", command.UserId);
        }
        else
        {
            // Cancel at period end in Stripe
            if (!string.IsNullOrEmpty(subscription.StripeSubscriptionId))
            {
                await _stripePaymentService.CancelSubscriptionAsync(subscription.StripeSubscriptionId);
            }

            subscription.CancelAtEndOfPeriod();
            _logger.LogInformation("Marked subscription for cancellation at period end for user: {UserId}",
                command.UserId);
        }

        await _subscriptionRepository.UpdateAsync(subscription);

        return MapToDto(subscription);
    }

    public async Task TrackFeatureUsageAsync(TrackFeatureUsageCommand command)
    {
        var tracking = await _usageTrackingRepository.GetByUserAndFeatureAsync(
            command.UserId,
            command.FeatureType);

        if (tracking == null)
        {
            // Create new tracking record
            var (periodStart, periodEnd) = GetPeriodForFeature(command.FeatureType);
            tracking = new UsageTracking(command.UserId, command.FeatureType, periodStart, periodEnd);
            tracking.IncrementUsage();
            await _usageTrackingRepository.CreateAsync(tracking);
        }
        else
        {
            // Check if period has expired
            if (tracking.IsPeriodExpired())
            {
                var (periodStart, periodEnd) = GetPeriodForFeature(command.FeatureType);
                tracking.ResetForNewPeriod(periodStart, periodEnd);
            }

            tracking.IncrementUsage();
            await _usageTrackingRepository.UpdateAsync(tracking);
        }

        _logger.LogInformation("Tracked usage for user: {UserId}, feature: {FeatureType}",
            command.UserId, command.FeatureType);
    }

    private (DateTime periodStart, DateTime periodEnd) GetPeriodForFeature(FeatureType featureType)
    {
        var now = DateTime.UtcNow;

        return featureType switch
        {
            FeatureType.AiChatMessage => (now.Date, now.Date.AddDays(1)),
            FeatureType.CheckIn => (now.Date, now.Date.AddDays(1)),
            FeatureType.FacialAnalysis => (GetStartOfWeek(now), GetStartOfWeek(now).AddDays(7)),
            FeatureType.ContentRecommendation => (GetStartOfWeek(now), GetStartOfWeek(now).AddDays(7)),
            FeatureType.ContentAssignment => (GetStartOfWeek(now), GetStartOfWeek(now).AddDays(7)),
            FeatureType.PatientConnection => (DateTime.MinValue, DateTime.MaxValue), // No time limit
            _ => throw new ArgumentException($"Invalid feature type: {featureType}")
        };
    }

    private DateTime GetStartOfWeek(DateTime date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-1 * diff).Date;
    }

    private SubscriptionDto MapToDto(Domain.Aggregates.Subscription subscription)
    {
        var limits = subscription.GetUsageLimits();

        return new SubscriptionDto
        {
            Id = subscription.Id,
            UserId = subscription.UserId,
            UserType = subscription.UserType,
            Plan = subscription.Plan,
            Status = subscription.Status,
            CurrentPeriodStart = subscription.CurrentPeriodStart,
            CurrentPeriodEnd = subscription.CurrentPeriodEnd,
            CancelAtPeriodEnd = subscription.CancelAtPeriodEnd,
            CancelledAt = subscription.CancelledAt,
            IsActive = subscription.IsActive(),
            UsageLimits = new UsageLimitsDto
            {
                AiChatMessagesPerDay = limits.AiChatMessagesPerDay,
                FacialAnalysisPerWeek = limits.FacialAnalysisPerWeek,
                ContentRecommendationsPerWeek = limits.ContentRecommendationsPerWeek,
                CheckInsPerDay = limits.CheckInsPerDay,
                MaxPatientConnections = limits.MaxPatientConnections,
                ContentAssignmentsPerWeek = limits.ContentAssignmentsPerWeek
            },
            CreatedAt = subscription.CreatedAt,
            UpdatedAt = subscription.UpdatedAt
        };
    }
}
