namespace SoftFocusBackend.Auth.Domain.Model.Commands;

public record CompleteOAuthRegistrationCommand
{
    public string TempToken { get; init; }
    public string Email { get; init; }
    public string FullName { get; init; }
    public string Provider { get; init; }
    public string UserType { get; init; }
    public bool AcceptsPrivacyPolicy { get; init; }

    // Professional fields - only for Psychologist
    public string? ProfessionalLicense { get; init; }
    public int? YearsOfExperience { get; init; }
    public string? CollegiateRegion { get; init; }
    public string[]? Specialties { get; init; }
    public string? University { get; init; }
    public int? GraduationYear { get; init; }
    public string? LicenseDocumentUrl { get; init; }
    public string? DiplomaDocumentUrl { get; init; }
    public string? DniDocumentUrl { get; init; }
    public string[]? CertificationDocumentUrls { get; init; }

    public DateTime RequestedAt { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }

    public CompleteOAuthRegistrationCommand(
        string tempToken,
        string email,
        string fullName,
        string provider,
        string userType,
        bool acceptsPrivacyPolicy,
        string? professionalLicense = null,
        int? yearsOfExperience = null,
        string? collegiateRegion = null,
        string[]? specialties = null,
        string? university = null,
        int? graduationYear = null,
        string? licenseDocumentUrl = null,
        string? diplomaDocumentUrl = null,
        string? dniDocumentUrl = null,
        string[]? certificationDocumentUrls = null,
        string? ipAddress = null,
        string? userAgent = null)
    {
        TempToken = tempToken;
        Email = email;
        FullName = fullName;
        Provider = provider;
        UserType = userType;
        AcceptsPrivacyPolicy = acceptsPrivacyPolicy;
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
        RequestedAt = DateTime.UtcNow;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }

    public bool IsValid()
    {
        var basicValid = !string.IsNullOrWhiteSpace(Email) &&
                        !string.IsNullOrWhiteSpace(FullName) &&
                        !string.IsNullOrWhiteSpace(Provider) &&
                        !string.IsNullOrWhiteSpace(UserType) &&
                        AcceptsPrivacyPolicy &&
                        Email.Contains('@') &&
                        (UserType == "General" || UserType == "Psychologist");

        if (!basicValid)
            return false;

        // Additional validation for Psychologist
        if (UserType == "Psychologist")
        {
            return !string.IsNullOrWhiteSpace(ProfessionalLicense) &&
                   !string.IsNullOrWhiteSpace(CollegiateRegion) &&
                   !string.IsNullOrWhiteSpace(University) &&
                   YearsOfExperience.HasValue && YearsOfExperience.Value >= 0 && YearsOfExperience.Value <= 70 &&
                   GraduationYear.HasValue && GraduationYear.Value >= 1950 && GraduationYear.Value <= DateTime.UtcNow.Year;
        }

        return true;
    }

    public string GetAuditString()
    {
        var parts = new List<string> { $"Email: {Email}", $"Provider: {Provider}", $"UserType: {UserType}" };

        if (!string.IsNullOrWhiteSpace(IpAddress))
            parts.Add($"IP: {IpAddress}");

        if (!string.IsNullOrWhiteSpace(UserAgent))
            parts.Add($"UserAgent: {UserAgent[..Math.Min(50, UserAgent.Length)]}...");

        parts.Add($"RequestedAt: {RequestedAt:yyyy-MM-dd HH:mm:ss} UTC");

        return string.Join(" | ", parts);
    }
}
