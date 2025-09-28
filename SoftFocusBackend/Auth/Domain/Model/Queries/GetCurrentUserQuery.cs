namespace SoftFocusBackend.Auth.Domain.Model.Queries;

public record GetCurrentUserQuery
{
    public string UserId { get; init; }
    public DateTime RequestedAt { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }

    public GetCurrentUserQuery(string userId, string? ipAddress = null, string? userAgent = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId cannot be null or empty.", nameof(userId));

        UserId = userId;
        RequestedAt = DateTime.UtcNow;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }

    public bool IsValid() => !string.IsNullOrWhiteSpace(UserId);
    
    public bool HasAuditInfo() => !string.IsNullOrWhiteSpace(IpAddress) || !string.IsNullOrWhiteSpace(UserAgent);
    
    public string GetAuditString()
    {
        var parts = new List<string> { $"UserId: {UserId}" };
        
        if (!string.IsNullOrWhiteSpace(IpAddress))
            parts.Add($"IP: {IpAddress}");
            
        if (!string.IsNullOrWhiteSpace(UserAgent))
            parts.Add($"UserAgent: {UserAgent[..Math.Min(50, UserAgent.Length)]}...");
            
        parts.Add($"RequestedAt: {RequestedAt:yyyy-MM-dd HH:mm:ss} UTC");
        
        return string.Join(" | ", parts);
    }
}