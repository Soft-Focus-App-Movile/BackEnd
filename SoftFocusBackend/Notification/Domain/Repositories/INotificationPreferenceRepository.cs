using SoftFocusBackend.Shared.Domain.Repositories;
using SoftFocusBackend.Notification.Domain.Model.Aggregates;

namespace SoftFocusBackend.Notification.Domain.Repositories;

public interface INotificationPreferenceRepository : IBaseRepository<NotificationPreference>
{
    Task<IEnumerable<NotificationPreference>> GetByUserIdAsync(string userId);
    Task<NotificationPreference?> GetByUserAndTypeAsync(string userId, string notificationType);
    Task CreateAsync(NotificationPreference preference);
    Task UpdateAsync(string preferenceId, NotificationPreference preference);
}