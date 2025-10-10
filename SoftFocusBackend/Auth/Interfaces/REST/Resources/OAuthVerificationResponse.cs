namespace SoftFocusBackend.Auth.Interfaces.REST.Resources;

public record OAuthVerificationResponse
{
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public string TempToken { get; init; } = string.Empty;
    public bool NeedsRegistration { get; init; }
    public string? ExistingUserType { get; init; }
}
