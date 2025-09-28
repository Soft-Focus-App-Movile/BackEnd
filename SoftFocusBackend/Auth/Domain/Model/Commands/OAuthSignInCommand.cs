using SoftFocusBackend.Auth.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Auth.Domain.Model.Commands;

public record OAuthSignInCommand
{
    public OAuthProvider Provider { get; init; }
    public DateTime RequestedAt { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }

    public OAuthSignInCommand(string providerName, string accessToken, string? refreshToken = null, DateTime expiresAt = default, string? ipAddress = null, string? userAgent = null)
    {
        Provider = new OAuthProvider(providerName, accessToken, refreshToken, expiresAt);
        RequestedAt = DateTime.UtcNow;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }

    public bool IsValid() => !Provider.IsExpired && !string.IsNullOrWhiteSpace(Provider.AccessToken);
    
    public bool HasAuditInfo() => !string.IsNullOrWhiteSpace(IpAddress) || !string.IsNullOrWhiteSpace(UserAgent);
    
    public string GetAuditString()
    {
        var parts = new List<string> { $"Provider: {Provider.Name}" };
        
        if (!string.IsNullOrWhiteSpace(IpAddress))
            parts.Add($"IP: {IpAddress}");
            
        if (!string.IsNullOrWhiteSpace(UserAgent))
            parts.Add($"UserAgent: {UserAgent[..Math.Min(50, UserAgent.Length)]}...");
            
        parts.Add($"RequestedAt: {RequestedAt:yyyy-MM-dd HH:mm:ss} UTC");
        
        return string.Join(" | ", parts);
    }
}