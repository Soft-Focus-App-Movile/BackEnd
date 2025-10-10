namespace SoftFocusBackend.Auth.Domain.Model.Commands;

public record RegisterPsychologistCommand
{
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string Email { get; init; }
    public string Password { get; init; }
    public string ProfessionalLicense { get; init; }
    public int YearsOfExperience { get; init; }
    public string CollegiateRegion { get; init; }
    public string[] Specialties { get; init; }
    public string University { get; init; }
    public int GraduationYear { get; init; }
    public string? LicenseDocumentUrl { get; init; }
    public string? DiplomaDocumentUrl { get; init; }
    public string? DniDocumentUrl { get; init; }
    public string[]? CertificationDocumentUrls { get; init; }
    public bool AcceptsPrivacyPolicy { get; init; }
    public DateTime RequestedAt { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }

    public RegisterPsychologistCommand(
        string firstName,
        string lastName,
        string email,
        string password,
        string professionalLicense,
        int yearsOfExperience,
        string collegiateRegion,
        string[] specialties,
        string university,
        int graduationYear,
        bool acceptsPrivacyPolicy,
        string? licenseDocumentUrl = null,
        string? diplomaDocumentUrl = null,
        string? dniDocumentUrl = null,
        string[]? certificationDocumentUrls = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Password = password;
        ProfessionalLicense = professionalLicense;
        YearsOfExperience = yearsOfExperience;
        CollegiateRegion = collegiateRegion;
        Specialties = specialties;
        University = university;
        GraduationYear = graduationYear;
        LicenseDocumentUrl = licenseDocumentUrl;
        DiplomaDocumentUrl = diplomaDocumentUrl;
        DniDocumentUrl = dniDocumentUrl;
        CertificationDocumentUrls = certificationDocumentUrls;
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
        !string.IsNullOrWhiteSpace(ProfessionalLicense) &&
        !string.IsNullOrWhiteSpace(CollegiateRegion) &&
        !string.IsNullOrWhiteSpace(University) &&
        YearsOfExperience >= 0 && YearsOfExperience <= 70 &&
        GraduationYear >= 1950 && GraduationYear <= DateTime.UtcNow.Year &&
        AcceptsPrivacyPolicy &&
        Email.Contains('@') &&
        Password.Length >= 6;

    public string GetAuditString()
    {
        var parts = new List<string> { $"Email: {Email}", $"License: {ProfessionalLicense}" };

        if (!string.IsNullOrWhiteSpace(IpAddress))
            parts.Add($"IP: {IpAddress}");

        if (!string.IsNullOrWhiteSpace(UserAgent))
            parts.Add($"UserAgent: {UserAgent[..Math.Min(50, UserAgent.Length)]}...");

        parts.Add($"RequestedAt: {RequestedAt:yyyy-MM-dd HH:mm:ss} UTC");

        return string.Join(" | ", parts);
    }
}
