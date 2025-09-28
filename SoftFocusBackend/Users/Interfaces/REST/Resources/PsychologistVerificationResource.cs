using SoftFocusBackend.Users.Domain.Model.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace SoftFocusBackend.Users.Interfaces.REST.Resources;

public record PsychologistVerificationResource
{
    [Required(ErrorMessage = "License number is required")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "License number must be between 3 and 20 characters")]
    public string LicenseNumber { get; init; } = string.Empty;

    [Required(ErrorMessage = "Professional college is required")]
    [StringLength(100, ErrorMessage = "Professional college cannot exceed 100 characters")]
    public string ProfessionalCollege { get; init; } = string.Empty;

    [StringLength(50, ErrorMessage = "College region cannot exceed 50 characters")]
    public string? CollegeRegion { get; init; }

    [Required(ErrorMessage = "At least one specialty is required")]
    [MinLength(1, ErrorMessage = "At least one specialty must be selected")]
    public List<PsychologySpecialty> Specialties { get; init; } = new();

    [Range(0, 50, ErrorMessage = "Years of experience must be between 0 and 50")]
    public int YearsOfExperience { get; init; }

    [StringLength(100, ErrorMessage = "University cannot exceed 100 characters")]
    public string? University { get; init; }

    [Range(1950, 2030, ErrorMessage = "Graduation year must be between 1950 and 2030")]
    public int? GraduationYear { get; init; }

    [StringLength(50, ErrorMessage = "Degree cannot exceed 50 characters")]
    public string? Degree { get; init; }

    [Url(ErrorMessage = "Invalid license document URL")]
    public string? LicenseDocumentUrl { get; init; }

    [Url(ErrorMessage = "Invalid diploma certificate URL")]
    public string? DiplomaCertificateUrl { get; init; }

    [Url(ErrorMessage = "Invalid identity document URL")]
    public string? IdentityDocumentUrl { get; init; }

    public List<string>? AdditionalCertificatesUrls { get; init; }

    public bool IsVerified { get; init; }
    public DateTime? VerificationDate { get; init; }
    public string? VerifiedBy { get; init; }
    public string? VerificationNotes { get; init; }
}