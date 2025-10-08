namespace SoftFocusBackend.Tracking.Application.ACL.Services;

public interface IUserValidationService
{
    Task<bool> ValidateUserExistsAsync(string userId);
    Task<bool> IsUserActiveAsync(string userId);
    Task<string?> GetUserFullNameAsync(string userId);
}