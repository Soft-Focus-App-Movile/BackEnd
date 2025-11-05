using SoftFocusBackend.Notification.Domain.Model.Queries;
using SoftFocusBackend.Notification.Domain.Model.Aggregates;
using SoftFocusBackend.Notification.Domain.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Alias para evitar conflictos de nombres
using NotificationAggregate = SoftFocusBackend.Notification.Domain.Model.Aggregates.Notification;

namespace SoftFocusBackend.Notification.Application.Internal.QueryServices
{
    public class NotificationHistoryQueryService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationHistoryQueryService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        // Historial por usuario
        public async Task<IEnumerable<NotificationAggregate>> HandleAsync(GetNotificationHistoryQuery query)
        {
            var notifications = await _notificationRepository.GetByUserIdAsync(query.UserId, query.Page, query.PageSize);

            if (!string.IsNullOrEmpty(query.Type))
                notifications = notifications.Where(n => n.Type == query.Type);

            if (query.StartDate.HasValue)
                notifications = notifications.Where(n => n.CreatedAt >= query.StartDate.Value);

            if (query.EndDate.HasValue)
                notifications = notifications.Where(n => n.CreatedAt <= query.EndDate.Value);

            return notifications;
        }

        // Obtener notificación por ID
        public async Task<NotificationAggregate?> HandleAsync(GetNotificationByIdQuery query)
        {
            return await _notificationRepository.GetByIdAsync(query.NotificationId);
        }

        //Obtener notificaciones no leídas
        public async Task<IEnumerable<NotificationAggregate>> HandleAsync(GetUnreadNotificationsQuery query)
        {
            return await _notificationRepository.GetUnreadByUserIdAsync(query.UserId);
        }
    }
}