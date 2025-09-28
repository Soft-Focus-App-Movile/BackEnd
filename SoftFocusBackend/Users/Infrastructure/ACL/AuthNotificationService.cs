using SoftFocusBackend.Auth.Domain.Model.ValueObjects;
using SoftFocusBackend.Users.Application.ACL.Services;

namespace SoftFocusBackend.Users.Infrastructure.ACL;

public class AuthNotificationService : IAuthNotificationService
{
    private readonly ILogger<AuthNotificationService> _logger;

    public AuthNotificationService(ILogger<AuthNotificationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AuthenticatedUser?> CreateAuthenticatedUserAsync(string userId, string email, string fullName, 
        string role, string? profileImageUrl = null, DateTime? lastLogin = null)
    {
        try
        {
            _logger.LogDebug("Creating authenticated user for: {UserId} - {Email}", userId, email);

            var authenticatedUser = new AuthenticatedUser(
                id: userId,
                fullName: fullName,
                email: email,
                role: role,
                profileImageUrl: profileImageUrl,
                lastLogin: lastLogin
            );

            _logger.LogDebug("Authenticated user created successfully for: {UserId}", userId);
            return authenticatedUser;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating authenticated user for: {UserId}", userId);
            return null;
        }
    }

    public async Task NotifyUserCreatedAsync(string userId, string email, string userType)
    {
        try
        {
            _logger.LogInformation("Notifying Auth context of user creation: {UserId} - {Email} - {UserType}", 
                userId, email, userType);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying user creation: {UserId}", userId);
        }
    }

    public async Task NotifyUserUpdatedAsync(string userId, string email)
    {
        try
        {
            _logger.LogDebug("Notifying Auth context of user update: {UserId} - {Email}", userId, email);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying user update: {UserId}", userId);
        }
    }

    public async Task NotifyUserDeletedAsync(string userId, string email)
    {
        try
        {
            _logger.LogInformation("Notifying Auth context of user deletion: {UserId} - {Email}", userId, email);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying user deletion: {UserId}", userId);
        }
    }

    public async Task NotifyPasswordChangedAsync(string userId, string email)
    {
        try
        {
            _logger.LogInformation("Notifying Auth context of password change: {UserId} - {Email}", userId, email);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error notifying password change: {UserId}", userId);
        }
    }

    public async Task<bool> ValidateUserExistsAsync(string userId)
    {
        try
        {
            _logger.LogDebug("Validating user exists in Auth context: {UserId}", userId);

            await Task.CompletedTask;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating user existence: {UserId}", userId);
            return false;
        }
    }
}