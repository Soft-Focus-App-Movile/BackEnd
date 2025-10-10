namespace SoftFocusBackend.Notification.Domain.Services;

public interface INotificationSchedulingService
{
    Task<DateTime> CalculateOptimalDeliveryTimeAsync(string userId, string notificationType);
    Task<bool> ShouldSendNowAsync(string userId, string notificationType);
}