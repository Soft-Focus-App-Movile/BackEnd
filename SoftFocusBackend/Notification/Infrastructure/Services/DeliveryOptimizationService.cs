using SoftFocusBackend.Notification.Domain.Services;
using SoftFocusBackend.Notification.Domain.Model.ValueObjects;
using SoftFocusBackend.Notification.Domain.Repositories;

namespace SoftFocusBackend.Notification.Infrastructure.Services;

public class DeliveryOptimizationService : IDeliveryOptimizationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationPreferenceRepository _preferenceRepository;

    public DeliveryOptimizationService(
        INotificationRepository notificationRepository,
        INotificationPreferenceRepository preferenceRepository)
    {
        _notificationRepository = notificationRepository;
        _preferenceRepository = preferenceRepository;
    }

    public async Task<string> DetermineOptimalMethodAsync(string userId, string notificationType)
    {
        var preference = await _preferenceRepository.GetByUserAndTypeAsync(userId, notificationType);
        
        if (!string.IsNullOrEmpty(preference?.DeliveryMethod))
            return preference.DeliveryMethod;
        
        // Default logic based on notification type
        return notificationType switch
        {
            "CrisisAlert" => DeliveryMethod.Push.ToString(),
            "MessageReceived" => DeliveryMethod.Push.ToString(),
            "CheckinReminder" => DeliveryMethod.Push.ToString(),
            "AssignmentDue" => DeliveryMethod.Email.ToString(),
            _ => DeliveryMethod.InApp.ToString()
        };
    }

    public async Task<bool> IsUserActiveAsync(string userId)
    {
        // Check recent notification interactions
        var recentNotifications = await _notificationRepository.GetByUserIdAsync(userId, 1, 10);
        
        var recentlyRead = recentNotifications
            .Where(n => n.ReadAt.HasValue && n.ReadAt.Value > DateTime.UtcNow.AddDays(-7))
            .Any();
        
        return recentlyRead;
    }

    public async Task RecordDeliveryMetricsAsync(string notificationId, bool success, TimeSpan deliveryTime)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId);
        
        if (notification != null)
        {
            notification.Metadata["delivery_success"] = success;
            notification.Metadata["delivery_time_ms"] = deliveryTime.TotalMilliseconds;
            
            await _notificationRepository.UpdateAsync(notificationId, notification);
        }
    }
}