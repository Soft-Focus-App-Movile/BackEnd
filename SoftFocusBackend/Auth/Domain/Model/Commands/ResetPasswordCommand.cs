namespace SoftFocusBackend.Auth.Domain.Model.Commands;

public record ResetPasswordCommand
{
    public string Token { get; init; }
    public string Email { get; init; }
    public string NewPassword { get; init; }
    public DateTime RequestedAt { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }

    public ResetPasswordCommand(string token, string email, string newPassword, string? ipAddress = null, string? userAgent = null)
    {
        Token = token?.Trim() ?? throw new ArgumentNullException(nameof(token));
        Email = email?.Trim().ToLowerInvariant() ?? throw new ArgumentNullException(nameof(email));
        NewPassword = newPassword ?? throw new ArgumentNullException(nameof(newPassword));
        RequestedAt = DateTime.UtcNow;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Token) && 
               !string.IsNullOrWhiteSpace(Email) && 
               Email.Contains('@') && 
               Email.Length <= 100 &&
               !string.IsNullOrWhiteSpace(NewPassword) &&
               NewPassword.Length >= 6 &&
               NewPassword.Length <= 100;
    }
    
    public bool HasAuditInfo() => !string.IsNullOrWhiteSpace(IpAddress) || !string.IsNullOrWhiteSpace(UserAgent);
    
    public string GetAuditString()
    {
        var parts = new List<string> 
        { 
            $"Email: {Email}",
            $"Token: {Token[..Math.Min(10, Token.Length)]}..."
        };
        
        if (!string.IsNullOrWhiteSpace(IpAddress))
            parts.Add($"IP: {IpAddress}");
            
        if (!string.IsNullOrWhiteSpace(UserAgent))
            parts.Add($"UserAgent: {UserAgent[..Math.Min(50, UserAgent.Length)]}...");
            
        parts.Add($"RequestedAt: {RequestedAt:yyyy-MM-dd HH:mm:ss} UTC");
        
        return string.Join(" | ", parts);
    }
    
    public bool IsPasswordComplex()
    {
        if (string.IsNullOrWhiteSpace(NewPassword)) return false;
        
        var hasUpper = NewPassword.Any(char.IsUpper);
        var hasLower = NewPassword.Any(char.IsLower);
        var hasDigit = NewPassword.Any(char.IsDigit);
        var hasSpecial = NewPassword.Any(c => "@$!%*?&".Contains(c));
        
        return hasUpper && hasLower && hasDigit && hasSpecial;
    }
}