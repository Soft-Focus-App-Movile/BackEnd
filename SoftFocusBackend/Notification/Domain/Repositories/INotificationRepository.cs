using SoftFocusBackend.Shared.Domain.Repositories;
using SoftFocusBackend.Notification.Domain.Model.Aggregates;

namespace SoftFocusBackend.Notification.Domain.Repositories;

public interface INotificationRepository : IBaseRepository<Model.Aggregates.Notification>
{
    Task<IEnumerable<Model.Aggregates.Notification>> GetByUserIdAsync(string userId, int page = 1, int pageSize = 20);
    Task<IEnumerable<Model.Aggregates.Notification>> GetUnreadByUserIdAsync(string userId);
    Task<IEnumerable<Model.Aggregates.Notification>> GetPendingNotificationsAsync();
    Task<IEnumerable<Model.Aggregates.Notification>> GetScheduledNotificationsAsync(DateTime currentTime);
    Task<int> GetUnreadCountAsync(string userId);
    public Task CreateAsync(Model.Aggregates.Notification notification);
    Task UpdateAsync(string notificationId, Model.Aggregates.Notification notification);
    Task<Model.Aggregates.Notification> GetByIdAsync(string notificationId);
    Task DeleteAsync(string id);
}