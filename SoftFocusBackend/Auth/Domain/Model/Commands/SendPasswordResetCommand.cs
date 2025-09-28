namespace SoftFocusBackend.Auth.Domain.Model.Commands;

public record SendPasswordResetCommand
{
    public string Email { get; init; }
    public DateTime RequestedAt { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }

    public SendPasswordResetCommand(string email, string? ipAddress = null, string? userAgent = null)
    {
        Email = email?.Trim().ToLowerInvariant() ?? throw new ArgumentNullException(nameof(email));
        RequestedAt = DateTime.UtcNow;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Email) && 
               Email.Contains('@') && 
               Email.Length <= 100;
    }
    
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