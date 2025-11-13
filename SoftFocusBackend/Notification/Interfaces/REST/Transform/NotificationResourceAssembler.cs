using SoftFocusBackend.Notification.Domain.Model.Aggregates;
using SoftFocusBackend.Notification.Interfaces.REST.Resources;

namespace SoftFocusBackend.Notification.Interfaces.REST.Transform;

public static class NotificationResourceAssembler
{
    // Convertir Notification a Resource
    public static NotificationResource ToResource(Domain.Model.Aggregates.Notification notification)
    {
        return new NotificationResource
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Type = notification.Type,
            Title = notification.Title,
            Content = notification.Content,
            Priority = notification.Priority,
            Status = notification.Status,
            DeliveryMethod = notification.DeliveryMethod,
            ScheduledAt = notification.ScheduledAt,
            DeliveredAt = notification.DeliveredAt,
            ReadAt = notification.ReadAt,
            CreatedAt = notification.CreatedAt
        };
    }

    // ✅ Convertir NotificationPreference a Resource (CON SNAKE_CASE)
    public static NotificationPreferenceResource ToResource(NotificationPreference preference)
    {
        ScheduleResource? scheduleResource = null;

        if (preference.Schedule != null)
        {
            scheduleResource = new ScheduleResource
            {
                StartTime = preference.Schedule.QuietHours.FirstOrDefault()?.StartTime ?? "09:00",
                EndTime = preference.Schedule.QuietHours.FirstOrDefault()?.EndTime ?? "22:00",
                DaysOfWeek = ConvertActiveDaysToDaysOfWeek(preference.Schedule.ActiveDays)
            };
        }

        return new NotificationPreferenceResource
        {
            Id = preference.Id,
            UserId = preference.UserId,
            NotificationType = preference.NotificationType,
            IsEnabled = preference.IsEnabled,
            DeliveryMethod = preference.DeliveryMethod,
            Schedule = scheduleResource,
            DisabledAt = preference.DisabledAt // ✅ NUEVO: Mapear el campo disabled_at
        };
    }

    // Helper: Convertir días activos (string) a days_of_week (int)
    private static List<int> ConvertActiveDaysToDaysOfWeek(List<string> activeDays)
    {
        if (activeDays == null || !activeDays.Any())
            return new List<int> { 1, 2, 3, 4, 5, 6, 7 }; // Default: todos los días

        var daysMapping = new Dictionary<string, int>
        {
            { "monday", 1 },
            { "tuesday", 2 },
            { "wednesday", 3 },
            { "thursday", 4 },
            { "friday", 5 },
            { "saturday", 6 },
            { "sunday", 7 }
        };

        return activeDays
            .Select(day => daysMapping.GetValueOrDefault(day.ToLower(), 0))
            .Where(dayNum => dayNum > 0)
            .ToList();
    }
}