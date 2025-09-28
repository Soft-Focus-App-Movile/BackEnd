using SoftFocusBackend.Auth.Domain.Model.Commands;
using SoftFocusBackend.Auth.Domain.Model.ValueObjects;
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
            }
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
}