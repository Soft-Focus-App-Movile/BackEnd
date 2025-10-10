namespace SoftFocusBackend.Notification.Application.Internal.EventHandlers;

public class NotificationSentEventHandler
{
    private readonly Domain.Services.IDeliveryOptimizationService _optimizationService;

    public NotificationSentEventHandler(Domain.Services.IDeliveryOptimizationService optimizationService)
    {
        _optimizationService = optimizationService;
    }

    public async Task HandleAsync(string notificationId, bool success, TimeSpan deliveryTime)
    {
        await _optimizationService.RecordDeliveryMetricsAsync(notificationId, success, deliveryTime);
    }
}