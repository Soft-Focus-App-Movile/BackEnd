using SoftFocusBackend.Auth.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Auth.Application.ACL.Services;

public interface IUserContextService
{
    Task<AuthenticatedUser?> AuthenticateUserAsync(string email, string password);
    Task<AuthenticatedUser?> GetUserByIdAsync(string userId);
    Task<AuthenticatedUser?> GetUserByEmailAsync(string email);
    Task<AuthenticatedUser?> CreateOrGetOAuthUserAsync(string email, string fullName, string provider);
    Task<bool> UpdateUserLastLoginAsync(string userId);
    Task<bool> IsUserActiveAsync(string email);
    Task<string?> GetUserProfileImageUrlAsync(string userId, int size = 200);
    Task SendPasswordResetEmailAsync(AuthenticatedUser user, string resetToken);
    Task<bool> ResetUserPasswordAsync(string userId, string newPassword);
    
    Task<AuthenticatedUser?> CreateUserAsync(string email, string password, string fullName, string userType,
        string? professionalLicense = null, string[]? specialties = null, string? collegiateRegion = null,
        string? university = null, int? graduationYear = null, int? yearsOfExperience = null,
        string? licenseDocumentUrl = null, string? diplomaCertificateUrl = null,
        string? identityDocumentUrl = null, string[]? additionalCertificatesUrls = null);
}