using SoftFocusBackend.Users.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Users.Domain.Model.Commands;

public record UpdatePsychologistVerificationCommand
{
    public string UserId { get; init; }
    public string LicenseNumber { get; init; }
    public string ProfessionalCollege { get; init; }
    public string? CollegeRegion { get; init; }
    public List<PsychologySpecialty> Specialties { get; init; }
    public int YearsOfExperience { get; init; }
    public string? University { get; init; }
    public int? GraduationYear { get; init; }
    public string? Degree { get; init; }
    public string? LicenseDocumentUrl { get; init; }
    public string? DiplomaCertificateUrl { get; init; }
    public string? IdentityDocumentUrl { get; init; }
    public List<string>? AdditionalCertificatesUrls { get; init; }
    public DateTime RequestedAt { get; init; }

    public UpdatePsychologistVerificationCommand(string userId, string licenseNumber, 
        string professionalCollege, List<PsychologySpecialty> specialties, int yearsOfExperience,
        string? collegeRegion = null, string? university = null, int? graduationYear = null,
        string? degree = null, string? licenseDocumentUrl = null, string? diplomaCertificateUrl = null,
        string? identityDocumentUrl = null, List<string>? additionalCertificatesUrls = null)
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        LicenseNumber = licenseNumber?.Trim() ?? throw new ArgumentNullException(nameof(licenseNumber));
        ProfessionalCollege = professionalCollege?.Trim() ?? throw new ArgumentNullException(nameof(professionalCollege));
        CollegeRegion = collegeRegion?.Trim();
        Specialties = specialties ?? throw new ArgumentNullException(nameof(specialties));
        YearsOfExperience = yearsOfExperience;
        University = university?.Trim();
        GraduationYear = graduationYear;
        Degree = degree?.Trim();
        LicenseDocumentUrl = licenseDocumentUrl?.Trim();
        DiplomaCertificateUrl = diplomaCertificateUrl?.Trim();
        IdentityDocumentUrl = identityDocumentUrl?.Trim();
        AdditionalCertificatesUrls = additionalCertificatesUrls;
        RequestedAt = DateTime.UtcNow;
    }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(UserId) &&
               !string.IsNullOrWhiteSpace(LicenseNumber) &&
               !string.IsNullOrWhiteSpace(ProfessionalCollege) &&
               Specialties.Count > 0 &&
               YearsOfExperience >= 0 &&
               (GraduationYear == null || GraduationYear > 1950);
    }

    public string GetAuditString()
    {
        return $"UserId: {UserId} | LicenseNumber: {LicenseNumber} | College: {ProfessionalCollege} | RequestedAt: {RequestedAt:yyyy-MM-dd HH:mm:ss} UTC";
    }
}