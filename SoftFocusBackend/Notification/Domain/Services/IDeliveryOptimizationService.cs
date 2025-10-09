namespace SoftFocusBackend.Notification.Domain.Services;

public interface IDeliveryOptimizationService
{
    Task<string> DetermineOptimalMethodAsync(string userId, string notificationType);
    Task<bool> IsUserActiveAsync(string userId);
    Task RecordDeliveryMetricsAsync(string notificationId, bool success, TimeSpan deliveryTime);
}