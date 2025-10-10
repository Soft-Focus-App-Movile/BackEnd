using SoftFocusBackend.Auth.Domain.Model.Commands;
using SoftFocusBackend.Auth.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Auth.Domain.Services;

public interface IAuthCommandService
{
    Task<AuthToken?> HandleSignInAsync(SignInCommand command);
    Task<AuthToken?> HandleOAuthSignInAsync(OAuthSignInCommand command);
    Task<bool> HandleSendPasswordResetAsync(SendPasswordResetCommand command);
    Task<bool> HandleResetPasswordAsync(ResetPasswordCommand command);

    // New registration methods
    Task<string?> HandleRegisterGeneralUserAsync(RegisterGeneralUserCommand command);
    Task<string?> HandleRegisterPsychologistAsync(RegisterPsychologistCommand command);

    // New OAuth two-step flow methods
    Task<OAuthVerificationResult?> HandleVerifyOAuthAsync(VerifyOAuthCommand command);
    Task<AuthToken?> HandleCompleteOAuthRegistrationAsync(CompleteOAuthRegistrationCommand command);
}

public record OAuthVerificationResult
{
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public string TempToken { get; init; } = string.Empty;
    public bool NeedsRegistration { get; init; }
    public string? ExistingUserType { get; init; }
}