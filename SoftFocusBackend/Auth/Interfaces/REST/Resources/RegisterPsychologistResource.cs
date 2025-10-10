using System.ComponentModel.DataAnnotations;

namespace SoftFocusBackend.Auth.Interfaces.REST.Resources;

public record RegisterPsychologistResource
{
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters")]
    public string FirstName { get; init; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters")]
    public string LastName { get; init; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).+$",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number and one special character")]
    public string Password { get; init; } = string.Empty;

    [Required(ErrorMessage = "Professional license is required")]
    [StringLength(50, ErrorMessage = "Professional license cannot exceed 50 characters")]
    public string ProfessionalLicense { get; init; } = string.Empty;

    [Required(ErrorMessage = "Years of experience is required")]
    [Range(0, 70, ErrorMessage = "Years of experience must be between 0 and 70")]
    public int YearsOfExperience { get; init; }

    [Required(ErrorMessage = "Collegiate region is required")]
    [StringLength(100, ErrorMessage = "Collegiate region cannot exceed 100 characters")]
    public string CollegiateRegion { get; init; } = string.Empty;

    [Required(ErrorMessage = "At least one specialty is required")]
    [MinLength(1, ErrorMessage = "At least one specialty is required")]
    public string[] Specialties { get; init; } = Array.Empty<string>();

    [Required(ErrorMessage = "University is required")]
    [StringLength(200, ErrorMessage = "University cannot exceed 200 characters")]
    public string University { get; init; } = string.Empty;

    [Required(ErrorMessage = "Graduation year is required")]
    [Range(1950, 2030, ErrorMessage = "Graduation year must be between 1950 and 2030")]
    public int GraduationYear { get; init; }

    // Note: File uploads will be handled separately via multipart/form-data
    // These fields represent the file metadata or IDs after upload
    public string? LicenseDocumentUrl { get; init; }
    public string? DiplomaDocumentUrl { get; init; }
    public string? DniDocumentUrl { get; init; }
    public string[]? CertificationDocumentUrls { get; init; }

    [Required(ErrorMessage = "You must accept the privacy policy")]
    [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the privacy policy")]
    public bool AcceptsPrivacyPolicy { get; init; }
}
