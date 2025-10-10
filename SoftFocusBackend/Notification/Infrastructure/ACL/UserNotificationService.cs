using SoftFocusBackend.Notification.Application.ACL.Services;
using SoftFocusBackend.Users.Application.Internal.OutboundServices;

namespace SoftFocusBackend.Notification.Infrastructure.ACL;

public class UserNotificationService : IUserNotificationService
{
    private readonly IUserFacade _userFacade;

    public UserNotificationService(IUserFacade userFacade)
    {
        _userFacade = userFacade;
    }

    public async Task<bool> UserExistsAsync(string userId)
    {
        return await _userFacade.UserExistsAsync(userId);
    }

    public async Task<string> GetUserEmailAsync(string userId)
    {
        return await _userFacade.GetUserEmailByIdAsync(userId) ?? string.Empty;
    }

    public async Task<string> GetUserPhoneAsync(string userId)
    {
        return await _userFacade.GetUserPhoneByIdAsync(userId) ?? string.Empty;
    }

    public async Task<Dictionary<string, string>> GetUserDeviceTokensAsync(string userId)
    {
        // This would typically fetch device tokens from user profile
        return new Dictionary<string, string>();
    }
}