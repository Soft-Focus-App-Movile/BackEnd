namespace SoftFocusBackend.Notification.Application.ACL.Services;

public interface ITherapyNotificationService
{
    Task<bool> HasUpcomingSessionAsync(string userId);
    Task<DateTime?> GetNextSessionTimeAsync(string userId);
}