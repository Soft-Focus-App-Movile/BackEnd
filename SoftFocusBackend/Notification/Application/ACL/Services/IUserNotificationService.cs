namespace SoftFocusBackend.Notification.Application.ACL.Services;

public interface IUserNotificationService
{
    Task<bool> UserExistsAsync(string userId);
    Task<string> GetUserEmailAsync(string userId);
    Task<string> GetUserPhoneAsync(string userId);
    Task<Dictionary<string, string>> GetUserDeviceTokensAsync(string userId);
}