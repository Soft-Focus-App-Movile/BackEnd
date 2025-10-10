using SoftFocusBackend.Shared.Domain.Repositories;
using SoftFocusBackend.Notification.Domain.Model.Aggregates;

namespace SoftFocusBackend.Notification.Domain.Repositories;

public interface INotificationTemplateRepository : IBaseRepository<NotificationTemplate>
{
    Task<NotificationTemplate?> GetByTypeAsync(string type);
}