namespace SoftFocusBackend.Auth.Domain.Model.ValueObjects;

public record OAuthProvider
{
    public string Name { get; init; }
    public string AccessToken { get; init; }
    public string? RefreshToken { get; init; }
    public DateTime ExpiresAt { get; init; }

    public OAuthProvider(string name, string accessToken, string? refreshToken = null, DateTime expiresAt = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Provider name cannot be null or empty.", nameof(name));
        
        if (string.IsNullOrWhiteSpace(accessToken))
            throw new ArgumentException("Access token cannot be null or empty.", nameof(accessToken));

        Name = name.Trim().ToLowerInvariant();
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        ExpiresAt = expiresAt == default ? DateTime.UtcNow.AddHours(1) : expiresAt;
    }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsGoogle => Name.Equals("google", StringComparison.OrdinalIgnoreCase);
    public bool IsFacebook => Name.Equals("facebook", StringComparison.OrdinalIgnoreCase);
}