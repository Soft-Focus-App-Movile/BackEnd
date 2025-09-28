namespace SoftFocusBackend.Auth.Domain.Model.Commands;

public record RegisterCommand
{
    public string Email { get; init; }
    public string Password { get; init; }
    public string FullName { get; init; }
    public string UserType { get; init; }
    public string? ProfessionalLicense { get; init; }
    public string[]? Specialties { get; init; }
    public DateTime RequestedAt { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }

    public RegisterCommand(string email, string password, string fullName, string userType, 
        string? professionalLicense = null, string[]? specialties = null, string? ipAddress = null, string? userAgent = null)
    {
        Email = email?.Trim().ToLowerInvariant() ?? throw new ArgumentNullException(nameof(email));
        Password = password ?? throw new ArgumentNullException(nameof(password));
        FullName = fullName?.Trim() ?? throw new ArgumentNullException(nameof(fullName));
        UserType = userType?.Trim() ?? throw new ArgumentNullException(nameof(userType));
        ProfessionalLicense = professionalLicense?.Trim();
        Specialties = specialties;
        RequestedAt = DateTime.UtcNow;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Email) && 
               Email.Contains('@') && 
               !string.IsNullOrWhiteSpace(Password) &&
               Password.Length >= 6 &&
               !string.IsNullOrWhiteSpace(FullName) &&
               (UserType == "General" || UserType == "Psychologist");
    }

    public bool IsPsychologist() => UserType.Equals("Psychologist", StringComparison.OrdinalIgnoreCase);

    public string GetAuditString()
    {
        var parts = new List<string> { $"Email: {Email}", $"UserType: {UserType}" };
        
        if (!string.IsNullOrWhiteSpace(IpAddress))
            parts.Add($"IP: {IpAddress}");
            
        parts.Add($"RequestedAt: {RequestedAt:yyyy-MM-dd HH:mm:ss} UTC");
        
        return string.Join(" | ", parts);
    }
}