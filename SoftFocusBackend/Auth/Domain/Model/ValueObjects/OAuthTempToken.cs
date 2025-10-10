namespace SoftFocusBackend.Auth.Domain.Model.ValueObjects;

/// <summary>
/// Represents a temporary token generated during OAuth verification
/// Used to link OAuth verification step with registration completion step
/// </summary>
public record OAuthTempToken
{
    public string Token { get; init; }
    public string Email { get; init; }
    public string FullName { get; init; }
    public string Provider { get; init; }
    public DateTime ExpiresAt { get; init; }

    public OAuthTempToken(string email, string fullName, string provider, int validMinutes = 15)
    {
        Email = email;
        FullName = fullName;
        Provider = provider;
        Token = GenerateToken();
        ExpiresAt = DateTime.UtcNow.AddMinutes(validMinutes);
    }

    private OAuthTempToken(string token, string email, string fullName, string provider, DateTime expiresAt)
    {
        Token = token;
        Email = email;
        FullName = fullName;
        Provider = provider;
        ExpiresAt = expiresAt;
    }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;

    public bool IsValid => !IsExpired && !string.IsNullOrWhiteSpace(Token);

    private static string GenerateToken()
    {
        // Generate a cryptographically secure random token
        var bytes = new byte[32];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }

    public static OAuthTempToken FromExisting(string token, string email, string fullName, string provider, DateTime expiresAt)
    {
        return new OAuthTempToken(token, email, fullName, provider, expiresAt);
    }
}
