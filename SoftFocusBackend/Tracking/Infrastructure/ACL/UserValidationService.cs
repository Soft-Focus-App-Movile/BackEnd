using SoftFocusBackend.Tracking.Application.ACL.Services;
using SoftFocusBackend.Users.Application.Internal.OutboundServices;

namespace SoftFocusBackend.Tracking.Infrastructure.ACL;

public class UserValidationService : IUserValidationService
{
    private readonly IUserFacade _userFacade;
    private readonly ILogger<UserValidationService> _logger;

    public UserValidationService(
        IUserFacade userFacade,
        ILogger<UserValidationService> logger)
    {
        _userFacade = userFacade ?? throw new ArgumentNullException(nameof(userFacade));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> ValidateUserExistsAsync(string userId)
    {
        try
        {
            _logger.LogDebug("Validating user exists: {UserId}", userId);

            var user = await _userFacade.GetUserByIdAsync(userId);
            var exists = user != null;

            _logger.LogDebug("User validation result for {UserId}: {Exists}", userId, exists);
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating user existence: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> IsUserActiveAsync(string userId)
    {
        try
        {
            _logger.LogDebug("Checking if user is active: {UserId}", userId);

            var user = await _userFacade.GetUserByIdAsync(userId);
            var isActive = user?.IsActive ?? false;

            _logger.LogDebug("User active status for {UserId}: {IsActive}", userId, isActive);
            return isActive;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking user active status: {UserId}", userId);
            return false;
        }
    }

    public async Task<string?> GetUserFullNameAsync(string userId)
    {
        try
        {
            _logger.LogDebug("Getting user full name: {UserId}", userId);

            var user = await _userFacade.GetUserByIdAsync(userId);
            var fullName = user?.FullName;

            _logger.LogDebug("User full name for {UserId}: {FullName}", userId, fullName ?? "Not found");
            return fullName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user full name: {UserId}", userId);
            return null;
        }
    }
}