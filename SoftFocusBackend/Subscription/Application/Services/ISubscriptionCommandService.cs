using SoftFocusBackend.Subscription.Application.Commands;
using SoftFocusBackend.Subscription.Application.DTOs;

namespace SoftFocusBackend.Subscription.Application.Services;

public interface ISubscriptionCommandService
{
    Task<SubscriptionDto> CreateBasicSubscriptionAsync(CreateBasicSubscriptionCommand command);
    Task<CheckoutSessionResponse> CreateProCheckoutSessionAsync(string userId, CreateCheckoutSessionRequest request);
    Task<SubscriptionDto> HandleSuccessfulCheckoutAsync(string sessionId);
    Task<SubscriptionDto> CancelSubscriptionAsync(CancelSubscriptionCommand command);
    Task TrackFeatureUsageAsync(TrackFeatureUsageCommand command);
}
