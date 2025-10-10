using System.ComponentModel.DataAnnotations;

namespace SoftFocusBackend.Auth.Interfaces.REST.Resources;

public record OAuthCompleteRegistrationResource
{
    [Required(ErrorMessage = "Temporary token is required")]
    public string TempToken { get; init; } = string.Empty;

    [Required(ErrorMessage = "User type is required")]
    [RegularExpression("^(General|Psychologist)$", ErrorMessage = "User type must be either 'General' or 'Psychologist'")]
    public string UserType { get; init; } = string.Empty;

    // Professional fields - required only if UserType == "Psychologist"
    public string? ProfessionalLicense { get; init; }
    public int? YearsOfExperience { get; init; }
    public string? CollegiateRegion { get; init; }
    public string[]? Specialties { get; init; }
    public string? University { get; init; }
    public int? GraduationYear { get; init; }

    // Document URLs - optional, can be uploaded later
    public string? LicenseDocumentUrl { get; init; }
    public string? DiplomaDocumentUrl { get; init; }
    public string? DniDocumentUrl { get; init; }
    public string[]? CertificationDocumentUrls { get; init; }

    [Required(ErrorMessage = "You must accept the privacy policy")]
    [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the privacy policy")]
    public bool AcceptsPrivacyPolicy { get; init; }
}
