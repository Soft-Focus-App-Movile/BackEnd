using SoftFocusBackend.Auth.Domain.Model.Commands;
using SoftFocusBackend.Auth.Domain.Model.ValueObjects;
using SoftFocusBackend.Auth.Domain.Services;
using SoftFocusBackend.Auth.Interfaces.REST.Resources;

namespace SoftFocusBackend.Auth.Interfaces.REST.Transform;

public static class AuthResourceAssembler
{
    public static SignInCommand ToCommand(SignInResource resource, string? ipAddress = null, string? userAgent = null)
    {
        return new SignInCommand(
            resource.Email,
            resource.Password,
            ipAddress,
            userAgent);
    }

    public static OAuthSignInCommand ToCommand(OAuthSignInResource resource, string? ipAddress = null, string? userAgent = null)
    {
        return new OAuthSignInCommand(
            resource.Provider,
            resource.AccessToken,
            resource.RefreshToken,
            resource.ExpiresAt ?? DateTime.UtcNow.AddHours(1),
            ipAddress,
            userAgent);
    }

    public static AuthenticatedUserResource ToResource(AuthenticatedUser user)
    {
        return new AuthenticatedUserResource
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            ProfileImageUrl = user.ProfileImageUrl,
            LastLogin = user.LastLogin,
            RoleDisplay = user.GetDisplayRole(),
            Capabilities = new UserCapabilitiesResource
            {
                CanManageUsers = user.CanManageUsers(),
                CanProvideTherapy = user.CanProvideTherapy(),
                CanAccessPremiumFeatures = user.CanAccessPremiumFeatures(),
                IsAdmin = user.IsAdmin(),
                IsPsychologist = user.IsPsychologist(),
                IsGeneral = user.IsGeneral()
            },
            IsVerified = user.IsVerified
        };
    }

    public static object ToSignInResponse(AuthToken authToken)
    {
        var userId = authToken.GetUserId();
        var fullName = authToken.GetClaimValue("full_name");
        var email = authToken.GetUserEmail();
        var role = authToken.GetUserRole();
        var profileImage = authToken.GetClaimValue("profile_image");
        var lastLoginStr = authToken.GetClaimValue("last_login");
        
        DateTime? lastLogin = null;
        if (!string.IsNullOrEmpty(lastLoginStr) && DateTime.TryParse(lastLoginStr, out var parsedLastLogin))
        {
            lastLogin = parsedLastLogin;
        }

        var user = new AuthenticatedUser(userId, fullName, email, role, profileImage, lastLogin);

        return new
        {
            user = ToResource(user),
            token = authToken.Token,
            expiresAt = authToken.ExpiresAt,
            tokenType = authToken.TokenType
        };
    }

    public static object ToErrorResponse(string message = "Invalid credentials")
    {
        return new
        {
            error = true,
            message = message,
            timestamp = DateTime.UtcNow
        };
    }

    public static object ToUnauthorizedResponse(string message = "Unauthorized")
    {
        return new
        {
            error = true,
            message = message,
            timestamp = DateTime.UtcNow
        };
    }
    
    public static RegisterCommand ToCommand(RegisterResource resource, string? ipAddress = null, string? userAgent = null)
    {
        return new RegisterCommand(
            resource.Email,
            resource.Password,
            resource.FullName,
            resource.UserType,
            resource.ProfessionalLicense,
            resource.Specialties,
            ipAddress,
            userAgent);
    }

    // New registration assemblers
    public static RegisterGeneralUserCommand ToCommand(RegisterGeneralUserResource resource, string? ipAddress = null, string? userAgent = null)
    {
        return new RegisterGeneralUserCommand(
            resource.FirstName,
            resource.LastName,
            resource.Email,
            resource.Password,
            resource.AcceptsPrivacyPolicy,
            ipAddress,
            userAgent);
    }

    public static RegisterPsychologistCommand ToCommand(RegisterPsychologistResource resource, string? ipAddress = null, string? userAgent = null)
    {
        return new RegisterPsychologistCommand(
            resource.FirstName,
            resource.LastName,
            resource.Email,
            resource.Password,
            resource.ProfessionalLicense,
            resource.YearsOfExperience,
            resource.CollegiateRegion,
            resource.Specialties,
            resource.University,
            resource.GraduationYear,
            resource.AcceptsPrivacyPolicy,
            resource.LicenseDocumentUrl,
            resource.DiplomaDocumentUrl,
            resource.DniDocumentUrl,
            resource.CertificationDocumentUrls,
            ipAddress,
            userAgent);
    }

    // OAuth verification assemblers
    public static VerifyOAuthCommand ToCommand(OAuthVerifyResource resource, string? ipAddress = null, string? userAgent = null)
    {
        return new VerifyOAuthCommand(
            resource.Provider,
            resource.AccessToken,
            resource.RefreshToken,
            resource.ExpiresAt ?? DateTime.UtcNow.AddHours(1),
            ipAddress,
            userAgent);
    }

    public static CompleteOAuthRegistrationCommand ToCommand(OAuthCompleteRegistrationResource resource, string tempToken, string email, string fullName, string provider, string? ipAddress = null, string? userAgent = null)
    {
        return new CompleteOAuthRegistrationCommand(
            tempToken,
            email,
            fullName,
            provider,
            resource.UserType,
            resource.AcceptsPrivacyPolicy,
            resource.ProfessionalLicense,
            resource.YearsOfExperience,
            resource.CollegiateRegion,
            resource.Specialties,
            resource.University,
            resource.GraduationYear,
            resource.LicenseDocumentUrl,
            resource.DiplomaDocumentUrl,
            resource.DniDocumentUrl,
            resource.CertificationDocumentUrls,
            ipAddress,
            userAgent);
    }

    public static OAuthVerificationResponse ToResource(OAuthVerificationResult result)
    {
        return new OAuthVerificationResponse
        {
            Email = result.Email,
            FullName = result.FullName,
            Provider = result.Provider,
            TempToken = result.TempToken,
            NeedsRegistration = result.NeedsRegistration,
            ExistingUserType = result.ExistingUserType
        };
    }
}