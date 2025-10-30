using SoftFocusBackend.Users.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Users.Domain.Model.Commands;

public record CreateUserCommand
{
    public string Email { get; init; }
    public string PasswordHash { get; init; }
    public string FullName { get; init; }
    public UserType UserType { get; init; }
    public string? ProfessionalLicense { get; init; }
    public List<PsychologySpecialty>? Specialties { get; init; }
    public string? CollegiateRegion { get; init; }
    public string? University { get; init; }
    public int? GraduationYear { get; init; }
    public int? YearsOfExperience { get; init; }
    public DateTime RequestedAt { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }

    public CreateUserCommand(string email, string passwordHash, string fullName, UserType userType,
        string? professionalLicense = null, List<PsychologySpecialty>? specialties = null,
        string? collegiateRegion = null, string? university = null, int? graduationYear = null,
        int? yearsOfExperience = null, string? ipAddress = null, string? userAgent = null)
    {
        Email = email?.Trim().ToLowerInvariant() ?? throw new ArgumentNullException(nameof(email));
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        FullName = fullName?.Trim() ?? throw new ArgumentNullException(nameof(fullName));
        UserType = userType;
        ProfessionalLicense = professionalLicense?.Trim();
        Specialties = specialties;
        CollegiateRegion = collegiateRegion?.Trim();
        University = university?.Trim();
        GraduationYear = graduationYear;
        YearsOfExperience = yearsOfExperience;
        RequestedAt = DateTime.UtcNow;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Email) &&
               Email.Contains('@') &&
               !string.IsNullOrWhiteSpace(PasswordHash) &&
               !string.IsNullOrWhiteSpace(FullName) &&
               Enum.IsDefined(typeof(UserType), UserType);
    }

    public bool IsPsychologist() => UserType == UserType.Psychologist;

    public string GetAuditString()
    {
        var parts = new List<string> { $"Email: {Email}", $"UserType: {UserType}" };

        if (!string.IsNullOrWhiteSpace(IpAddress))
            parts.Add($"IP: {IpAddress}");

        parts.Add($"RequestedAt: {RequestedAt:yyyy-MM-dd HH:mm:ss} UTC");

        return string.Join(" | ", parts);
    }
}