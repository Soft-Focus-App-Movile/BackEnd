namespace SoftFocusBackend.Auth.Interfaces.REST.Resources;

public record PasswordResetResponse
{
    public string Message { get; init; } = string.Empty;
    public bool Success { get; init; }
}