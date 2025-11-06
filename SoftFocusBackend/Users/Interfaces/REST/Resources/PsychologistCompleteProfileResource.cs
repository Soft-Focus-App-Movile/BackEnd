using SoftFocusBackend.Users.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Users.Interfaces.REST.Resources;

/// <summary>
/// Complete psychologist profile resource including all user data, verification data, and professional data
/// </summary>
public record PsychologistCompleteProfileResource
{
    // Basic User Data (from User aggregate)
    public string Id { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string UserType { get; init; } = string.Empty;
    public DateTime? DateOfBirth { get; init; }
    public string? Gender { get; init; }
    public string? Phone { get; init; }
    public string? ProfileImageUrl { get; init; }
    public string? Bio { get; init; }
    public string? Country { get; init; }
    public string? City { get; init; }
    public List<string>? Interests { get; init; }
    public List<string>? MentalHealthGoals { get; init; }
    public bool EmailNotifications { get; init; }
    public bool PushNotifications { get; init; }
    public bool IsProfilePublic { get; init; }
    public bool IsActive { get; init; }
    public DateTime? LastLogin { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }

    // Verification Data (from PsychologistUser)
    public string LicenseNumber { get; init; } = string.Empty;
    public string ProfessionalCollege { get; init; } = string.Empty;
    public string? CollegeRegion { get; init; }
    public List<PsychologySpecialty> Specialties { get; init; } = new();
    public int YearsOfExperience { get; init; }
    public string? University { get; init; }
    public int? GraduationYear { get; init; }
    public string? Degree { get; init; }
    public string? LicenseDocumentUrl { get; init; }
    public string? DiplomaCertificateUrl { get; init; }
    public string? IdentityDocumentUrl { get; init; }
    public List<string>? AdditionalCertificatesUrls { get; init; }
    public bool IsVerified { get; init; }
    public DateTime? VerificationDate { get; init; }
    public string? VerifiedBy { get; init; }
    public string? VerificationNotes { get; init; }

    // Professional Data (from PsychologistUser)
    public string? ProfessionalBio { get; init; }
    public bool IsAcceptingNewPatients { get; init; }
    public int? MaxPatientsCapacity { get; init; }
    public int? CurrentPatientsCount { get; init; }
    public List<string>? TargetAudience { get; init; }
    public List<string>? Languages { get; init; }
    public string? BusinessName { get; init; }
    public string? BusinessAddress { get; init; }
    public string? BankAccount { get; init; }
    public string? PaymentMethods { get; init; }
    public string? Currency { get; init; }
    public bool IsProfileVisibleInDirectory { get; init; }
    public bool AllowsDirectMessages { get; init; }
    public double? AverageRating { get; init; }
    public int? TotalReviews { get; init; }
}
