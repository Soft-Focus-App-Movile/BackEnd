namespace SoftFocusBackend.Auth.Infrastructure.OAuth.Services;

public interface IOAuthService
{
    Task<(string Email, string FullName, string? ProfileImageUrl)?> GetUserInfoAsync(string accessToken);
    Task<bool> ValidateTokenAsync(string accessToken);
    string GetProviderName();
}