using SoftFocusBackend.Notification.Domain.Services;
using SoftFocusBackend.Notification.Domain.Repositories;

namespace SoftFocusBackend.Notification.Infrastructure.Services;

public class NotificationSchedulingService : INotificationSchedulingService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationPreferenceRepository _preferenceRepository;

    public NotificationSchedulingService(
        INotificationRepository notificationRepository,
        INotificationPreferenceRepository preferenceRepository)
    {
        _notificationRepository = notificationRepository;
        _preferenceRepository = preferenceRepository;
    }

    public async Task<DateTime> CalculateOptimalDeliveryTimeAsync(string userId, string notificationType)
    {
        var preference = await _preferenceRepository.GetByUserAndTypeAsync(userId, notificationType);
        
        if (preference?.Schedule?.QuietHours != null && preference.Schedule.QuietHours.Any())
        {
            var now = DateTime.UtcNow;
            var userTime = TimeZoneInfo.ConvertTimeFromUtc(now, TimeZoneInfo.FindSystemTimeZoneById(preference.Schedule.TimeZone ?? "UTC"));
            
            // Check if current time is in quiet hours
            foreach (var quietHour in preference.Schedule.QuietHours)
            {
                var startTime = TimeSpan.Parse(quietHour.StartTime);
                var endTime = TimeSpan.Parse(quietHour.EndTime);
                var currentTime = userTime.TimeOfDay;
                
                if (currentTime >= startTime && currentTime <= endTime)
                {
                    // Schedule for after quiet hours
                    var scheduledTime = userTime.Date.Add(endTime).AddMinutes(15);
                    return TimeZoneInfo.ConvertTimeToUtc(scheduledTime, TimeZoneInfo.FindSystemTimeZoneById(preference.Schedule.TimeZone ?? "UTC"));
                }
            }
        }
        
        // Default to immediate delivery
        return DateTime.UtcNow;
    }

    public async Task<bool> ShouldSendNowAsync(string userId, string notificationType)
    {
        var preference = await _preferenceRepository.GetByUserAndTypeAsync(userId, notificationType);
        
        if (preference?.Schedule == null)
            return true;
        
        var now = DateTime.UtcNow;
        var userTime = TimeZoneInfo.ConvertTimeFromUtc(now, TimeZoneInfo.FindSystemTimeZoneById(preference.Schedule.TimeZone ?? "UTC"));
        
        // Check active days
        if (preference.Schedule.ActiveDays != null && preference.Schedule.ActiveDays.Any())
        {
            if (!preference.Schedule.ActiveDays.Contains(userTime.DayOfWeek.ToString()))
                return false;
        }
        
        // Check quiet hours
        if (preference.Schedule.QuietHours != null)
        {
            foreach (var quietHour in preference.Schedule.QuietHours)
            {
                var startTime = TimeSpan.Parse(quietHour.StartTime);
                var endTime = TimeSpan.Parse(quietHour.EndTime);
                var currentTime = userTime.TimeOfDay;
                
                if (currentTime >= startTime && currentTime <= endTime)
                    return false;
            }
        }
        
        return true;
    }
}