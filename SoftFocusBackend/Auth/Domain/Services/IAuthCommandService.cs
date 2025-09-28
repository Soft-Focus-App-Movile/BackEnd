using SoftFocusBackend.Auth.Domain.Model.Commands;
using SoftFocusBackend.Auth.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Auth.Domain.Services;

public interface IAuthCommandService
{
    Task<AuthToken?> HandleSignInAsync(SignInCommand command);
    Task<AuthToken?> HandleOAuthSignInAsync(OAuthSignInCommand command);
    Task<bool> HandleSendPasswordResetAsync(SendPasswordResetCommand command);
    Task<bool> HandleResetPasswordAsync(ResetPasswordCommand command);
}