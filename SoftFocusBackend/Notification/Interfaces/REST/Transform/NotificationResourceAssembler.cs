using SoftFocusBackend.Notification.Interfaces.REST.Resources;
using SoftFocusBackend.Notification.Domain.Model.Aggregates;
using System.Linq;

// Alias para evitar conflictos
using NotificationAggregate = SoftFocusBackend.Notification.Domain.Model.Aggregates.Notification;

namespace SoftFocusBackend.Notification.Interfaces.REST.Transform
{
    public static class NotificationResourceAssembler
    {
        public static NotificationResource ToResource(NotificationAggregate notification)
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

        public static NotificationPreferenceResource ToResource(NotificationPreference preference)
        {
            return new NotificationPreferenceResource
            {
                Id = preference.Id,
                NotificationType = preference.NotificationType,
                IsEnabled = preference.IsEnabled,
                DeliveryMethod = preference.DeliveryMethod,
                Schedule = preference.Schedule != null ? new NotificationPreferenceResource.ScheduleResource
                {
                    QuietHours = preference.Schedule.QuietHours.Select(qh => new NotificationPreferenceResource.QuietHourResource
                    {
                        StartTime = qh.StartTime,
                        EndTime = qh.EndTime
                    }).ToList(),
                    ActiveDays = preference.Schedule.ActiveDays,
                    TimeZone = preference.Schedule.TimeZone
                } : null
            };
        }
    }
}
