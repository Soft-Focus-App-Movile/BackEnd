using SoftFocusBackend.Auth.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Auth.Domain.Model.Commands;

public record SignInCommand
{
    public LoginCredentials Credentials { get; init; }
    public DateTime RequestedAt { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }

    public SignInCommand(string email, string password, string? ipAddress = null, string? userAgent = null)
    {
        Credentials = new LoginCredentials(email, password);
        RequestedAt = DateTime.UtcNow;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }

    public SignInCommand(LoginCredentials credentials, string? ipAddress = null, string? userAgent = null)
    {
        Credentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
        RequestedAt = DateTime.UtcNow;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }

    public string Email => Credentials.Email;
    public string Password => Credentials.Password;
    
    public bool IsValid() => Credentials.IsValid();
    
    public bool HasAuditInfo() => !string.IsNullOrWhiteSpace(IpAddress) || !string.IsNullOrWhiteSpace(UserAgent);
    
    public string GetAuditString()
    {
        var parts = new List<string> { $"Email: {Email}" };
        
        if (!string.IsNullOrWhiteSpace(IpAddress))
            parts.Add($"IP: {IpAddress}");
            
        if (!string.IsNullOrWhiteSpace(UserAgent))
            parts.Add($"UserAgent: {UserAgent[..Math.Min(50, UserAgent.Length)]}...");
            
        parts.Add($"RequestedAt: {RequestedAt:yyyy-MM-dd HH:mm:ss} UTC");
        
        return string.Join(" | ", parts);
    }
}