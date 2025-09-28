using SoftFocusBackend.Auth.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Users.Application.ACL.Services;

public interface IAuthNotificationService
{
    Task<AuthenticatedUser?> CreateAuthenticatedUserAsync(string userId, string email, string fullName, string role, 
        string? profileImageUrl = null, DateTime? lastLogin = null);
    Task NotifyUserCreatedAsync(string userId, string email, string userType);
    Task NotifyUserUpdatedAsync(string userId, string email);
    Task NotifyUserDeletedAsync(string userId, string email);
    Task NotifyPasswordChangedAsync(string userId, string email);
    Task<bool> ValidateUserExistsAsync(string userId);
}