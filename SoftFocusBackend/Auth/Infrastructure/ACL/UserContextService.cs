using SoftFocusBackend.Auth.Application.ACL.Services;
using SoftFocusBackend.Auth.Domain.Model.ValueObjects;
using SoftFocusBackend.Users.Application.Internal.OutboundServices;
using SoftFocusBackend.Shared.Infrastructure.ExternalServices.Email.Services;
using BCrypt.Net;

namespace SoftFocusBackend.Auth.Infrastructure.ACL;

public class UserContextService : IUserContextService
{
    private readonly IUserFacade _userFacade;
    private readonly IGenericEmailService _emailService;
    private readonly ILogger<UserContextService> _logger;

    public UserContextService(
        IUserFacade userFacade, 
        IGenericEmailService emailService,
        ILogger<UserContextService> logger)
    {
        _userFacade = userFacade ?? throw new ArgumentNullException(nameof(userFacade));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<AuthenticatedUser?> AuthenticateUserAsync(string email, string password)
    {
        try
        {
            var user = await _userFacade.GetUserByEmailAsync(email);
            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("Authentication failed - user not found or inactive: {Email}", email);
                return null;
            }

            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                _logger.LogWarning("Authentication failed - user has no password (OAuth user): {Email}", email);
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                _logger.LogWarning("Authentication failed - invalid password: {Email}", email);
                return null;
            }

            bool? isVerified = user is SoftFocusBackend.Users.Domain.Model.Aggregates.PsychologistUser psychologist
                ? psychologist.IsVerified
                : null;

            return new AuthenticatedUser(
                user.Id,
                user.FullName,
                user.Email,
                user.UserType.ToString(),
                user.ProfileImageUrl,
                user.LastLogin,
                isVerified
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication for email: {Email}", email);
            return null;
        }
    }

    public async Task<AuthenticatedUser?> GetUserByIdAsync(string userId)
    {
        try
        {
            var user = await _userFacade.GetUserByIdAsync(userId);
            if (user == null || !user.IsActive)
                return null;

            bool? isVerified = user is SoftFocusBackend.Users.Domain.Model.Aggregates.PsychologistUser psychologist
                ? psychologist.IsVerified
                : null;

            return new AuthenticatedUser(
                user.Id,
                user.FullName,
                user.Email,
                user.UserType.ToString(),
                user.ProfileImageUrl,
                user.LastLogin,
                isVerified
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID: {UserId}", userId);
            return null;
        }
    }

    public async Task<AuthenticatedUser?> GetUserByEmailAsync(string email)
    {
        try
        {
            var user = await _userFacade.GetUserByEmailAsync(email);
            if (user == null || !user.IsActive)
                return null;

            bool? isVerified = user is SoftFocusBackend.Users.Domain.Model.Aggregates.PsychologistUser psychologist
                ? psychologist.IsVerified
                : null;

            return new AuthenticatedUser(
                user.Id,
                user.FullName,
                user.Email,
                user.UserType.ToString(),
                user.ProfileImageUrl,
                user.LastLogin,
                isVerified
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email: {Email}", email);
            return null;
        }
    }

    public async Task<AuthenticatedUser?> CreateOrGetOAuthUserAsync(string email, string fullName, string provider)
    {
        try
        {
            _logger.LogInformation("Creating or getting OAuth user for email: {Email}, provider: {Provider}", email, provider);

            // Buscar si ya existe
            var existingUser = await _userFacade.GetUserByEmailAsync(email);
            if (existingUser != null)
            {
                bool? isVerifiedExisting = existingUser is SoftFocusBackend.Users.Domain.Model.Aggregates.PsychologistUser psychologistExisting
                    ? psychologistExisting.IsVerified
                    : null;

                return new AuthenticatedUser(
                    existingUser.Id,
                    existingUser.FullName,
                    existingUser.Email,
                    existingUser.UserType.ToString(),
                    existingUser.ProfileImageUrl,
                    existingUser.LastLogin,
                    isVerifiedExisting
                );
            }

            // Crear nuevo usuario OAuth
            var newUser = await _userFacade.CreateOAuthUserAsync(email, fullName);
            if (newUser == null)
            {
                _logger.LogError("Failed to create OAuth user for email: {Email}", email);
                return null;
            }

            bool? isVerifiedNew = newUser is SoftFocusBackend.Users.Domain.Model.Aggregates.PsychologistUser psychologistNew
                ? psychologistNew.IsVerified
                : null;

            return new AuthenticatedUser(
                newUser.Id,
                newUser.FullName,
                newUser.Email,
                newUser.UserType.ToString(),
                newUser.ProfileImageUrl,
                newUser.LastLogin,
                isVerifiedNew
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating/getting OAuth user for email: {Email}", email);
            return null;
        }
    }

    public async Task<bool> UpdateUserLastLoginAsync(string userId)
    {
        try
        {
            // TODO: Implementar en UserFacade
            _logger.LogWarning("UpdateUserLastLoginAsync not implemented yet in UserFacade");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating last login for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> IsUserActiveAsync(string email)
    {
        try
        {
            var user = await _userFacade.GetUserByEmailAsync(email);
            return user?.IsActive == true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking user active status for email: {Email}", email);
            return false;
        }
    }

    public async Task<string?> GetUserProfileImageUrlAsync(string userId, int size = 200)
    {
        try
        {
            var user = await _userFacade.GetUserByIdAsync(userId);
            return user?.ProfileImageUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting profile image URL for user: {UserId}", userId);
            return null;
        }
    }

    public async Task SendPasswordResetEmailAsync(AuthenticatedUser user, string resetToken)
    {
        try
        {
            await _emailService.SendPasswordResetEmailAsync(user.Email, user.FullName, resetToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending password reset email for user: {UserId}", user.Id);
            throw;
        }
    }

    public async Task<bool> ResetUserPasswordAsync(string userId, string newPassword)
    {
        try
        {
            // TODO: Implementar reset password en UserFacade
            _logger.LogWarning("ResetUserPasswordAsync not implemented yet in UserFacade");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for user: {UserId}", userId);
            return false;
        }
    }
    
    public async Task<AuthenticatedUser?> CreateUserAsync(string email, string password, string fullName, string userType,
        string? professionalLicense = null, string[]? specialties = null, string? collegiateRegion = null,
        string? university = null, int? graduationYear = null, int? yearsOfExperience = null,
        string? licenseDocumentUrl = null, string? diplomaCertificateUrl = null,
        string? identityDocumentUrl = null, string[]? additionalCertificatesUrls = null)
    {
        try
        {
            var user = await _userFacade.CreateUserAsync(email, password, fullName, userType,
                professionalLicense, specialties, collegiateRegion, university, graduationYear, yearsOfExperience,
                licenseDocumentUrl, diplomaCertificateUrl, identityDocumentUrl, additionalCertificatesUrls);

            if (user == null)
                return null;

            bool? isVerified = user is SoftFocusBackend.Users.Domain.Model.Aggregates.PsychologistUser psychologist
                ? psychologist.IsVerified
                : null;

            return new AuthenticatedUser(
                user.Id,
                user.FullName,
                user.Email,
                user.UserType.ToString(),
                user.ProfileImageUrl,
                user.LastLogin,
                isVerified
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {Email}", email);
            return null;
        }
    }
}