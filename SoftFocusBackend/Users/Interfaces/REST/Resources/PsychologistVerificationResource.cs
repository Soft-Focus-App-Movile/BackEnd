using SoftFocusBackend.Users.Domain.Model.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace SoftFocusBackend.Users.Interfaces.REST.Resources;

public class PsychologistVerificationResource
{
    [Required(ErrorMessage = "License number is required")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "License number must be between 3 and 20 characters")]
    public string LicenseNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Professional college is required")]
    [StringLength(100, ErrorMessage = "Professional college cannot exceed 100 characters")]
    public string ProfessionalCollege { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "College region cannot exceed 50 characters")]
    public string? CollegeRegion { get; set; }

    [Required(ErrorMessage = "At least one specialty is required")]
    [MinLength(1, ErrorMessage = "At least one specialty must be selected")]
    public List<PsychologySpecialty> Specialties { get; set; } = new();

    [Range(0, 50, ErrorMessage = "Years of experience must be between 0 and 50")]
    public int YearsOfExperience { get; set; }

    [StringLength(100, ErrorMessage = "University cannot exceed 100 characters")]
    public string? University { get; set; }

    [Range(1950, 2030, ErrorMessage = "Graduation year must be between 1950 and 2030")]
    public int? GraduationYear { get; set; }

    [StringLength(50, ErrorMessage = "Degree cannot exceed 50 characters")]
    public string? Degree { get; set; }

    [Url(ErrorMessage = "Invalid license document URL")]
    public string? LicenseDocumentUrl { get; set; }

    [Url(ErrorMessage = "Invalid diploma certificate URL")]
    public string? DiplomaCertificateUrl { get; set; }

    [Url(ErrorMessage = "Invalid identity document URL")]
    public string? IdentityDocumentUrl { get; set; }

    public List<string>? AdditionalCertificatesUrls { get; set; }

    public bool IsVerified { get; set; }
    public DateTime? VerificationDate { get; set; }
    public string? VerifiedBy { get; set; }
    public string? VerificationNotes { get; set; }

    // File upload properties
    public IFormFile? LicenseDocumentFile { get; set; }
    public IFormFile? DiplomaCertificateFile { get; set; }
    public IFormFile? IdentityDocumentFile { get; set; }
    public IFormFileCollection? AdditionalCertificatesFiles { get; set; }
}