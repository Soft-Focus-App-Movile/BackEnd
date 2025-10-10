namespace SoftFocusBackend.Auth.Domain.Model.Commands;

public record RegisterGeneralUserCommand
{
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string Email { get; init; }
    public string Password { get; init; }
    public bool AcceptsPrivacyPolicy { get; init; }
    public DateTime RequestedAt { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }

    public RegisterGeneralUserCommand(
        string firstName,
        string lastName,
        string email,
        string password,
        bool acceptsPrivacyPolicy,
        string? ipAddress = null,
        string? userAgent = null)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Password = password;
        AcceptsPrivacyPolicy = acceptsPrivacyPolicy;
        RequestedAt = DateTime.UtcNow;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }

    public string GetFullName() => $"{FirstName} {LastName}".Trim();

    public bool IsValid() =>
        !string.IsNullOrWhiteSpace(FirstName) &&
        !string.IsNullOrWhiteSpace(LastName) &&
        !string.IsNullOrWhiteSpace(Email) &&
        !string.IsNullOrWhiteSpace(Password) &&
        AcceptsPrivacyPolicy &&
        Email.Contains('@') &&
        Password.Length >= 6;

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
