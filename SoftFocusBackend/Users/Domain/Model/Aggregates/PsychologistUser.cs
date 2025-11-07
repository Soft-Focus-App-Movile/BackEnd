using MongoDB.Bson.Serialization.Attributes;
using SoftFocusBackend.Users.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Users.Domain.Model.Aggregates;

public class PsychologistUser : User
{
    [BsonElement("licenseNumber")]
    public string LicenseNumber { get; set; } = string.Empty;

    [BsonElement("professionalCollege")]
    public string ProfessionalCollege { get; set; } = string.Empty;

    [BsonElement("collegeRegion")]
    public string? CollegeRegion { get; set; }

    [BsonElement("specialties")]
    public List<PsychologySpecialty> Specialties { get; set; } = new();

    [BsonElement("yearsOfExperience")]
    public int YearsOfExperience { get; set; }

    [BsonElement("university")]
    public string? University { get; set; }

    [BsonElement("graduationYear")]
    public int? GraduationYear { get; set; }

    [BsonElement("degree")]
    public string? Degree { get; set; }

    [BsonElement("invitationCode")]
    public string InvitationCode { get; set; } = string.Empty;

    [BsonElement("invitationCodeGeneratedAt")]
    public DateTime InvitationCodeGeneratedAt { get; set; }

    [BsonElement("invitationCodeExpiresAt")]
    public DateTime InvitationCodeExpiresAt { get; set; }

    [BsonElement("isVerified")]
    public bool IsVerified { get; set; } = false;

    [BsonElement("verificationDate")]
    public DateTime? VerificationDate { get; set; }

    [BsonElement("verifiedBy")]
    public string? VerifiedBy { get; set; }

    [BsonElement("verificationNotes")]
    public string? VerificationNotes { get; set; }

    [BsonElement("licenseDocumentUrl")]
    public string? LicenseDocumentUrl { get; set; }

    [BsonElement("diplomaCertificateUrl")]
    public string? DiplomaCertificateUrl { get; set; }

    [BsonElement("identityDocumentUrl")]
    public string? IdentityDocumentUrl { get; set; }

    [BsonElement("additionalCertificatesUrls")]
    public List<string>? AdditionalCertificatesUrls { get; set; }

    [BsonElement("currency")]
    public string? Currency { get; set; } = "PEN";

    [BsonElement("languages")]
    public List<string>? Languages { get; set; }

    [BsonElement("professionalBio")]
    public string? ProfessionalBio { get; set; }

    [BsonElement("isAcceptingNewPatients")]
    public bool IsAcceptingNewPatients { get; set; } = true;

    [BsonElement("maxPatientsCapacity")]
    public int? MaxPatientsCapacity { get; set; }

    [BsonElement("currentPatientsCount")]
    public int? CurrentPatientsCount { get; set; } = 0;

    [BsonElement("targetAudience")]
    public List<string>? TargetAudience { get; set; }

    [BsonElement("averageRating")]
    public double? AverageRating { get; set; }

    [BsonElement("totalReviews")]
    public int? TotalReviews { get; set; } = 0;

    [BsonElement("isProfileVisibleInDirectory")]
    public bool IsProfileVisibleInDirectory { get; set; } = true;

    [BsonElement("allowsDirectMessages")]
    public bool AllowsDirectMessages { get; set; } = true;

    [BsonElement("businessName")]
    public string? BusinessName { get; set; }

    [BsonElement("businessAddress")]
    public string? BusinessAddress { get; set; }

    [BsonElement("bankAccount")]
    public string? BankAccount { get; set; }

    [BsonElement("paymentMethods")]
    public string? PaymentMethods { get; set; }

    [BsonElement("whatsApp")]
    public string? WhatsApp { get; set; }

    [BsonElement("corporateEmail")]
    public string? CorporateEmail { get; set; }

    public void GenerateNewInvitationCode()
    {
        InvitationCode = GenerateRandomCode();
        InvitationCodeGeneratedAt = DateTime.UtcNow;
        InvitationCodeExpiresAt = DateTime.UtcNow.AddDays(1);
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsInvitationCodeExpired()
    {
        return DateTime.UtcNow > InvitationCodeExpiresAt;
    }

    public void Verify(string verifiedBy, string? notes = null)
    {
        IsVerified = true;
        VerificationDate = DateTime.UtcNow;
        VerifiedBy = verifiedBy;
        VerificationNotes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateVerificationDocuments(string? licenseUrl = null, string? diplomaUrl = null, 
        string? identityUrl = null, List<string>? additionalUrls = null)
    {
        if (licenseUrl != null) LicenseDocumentUrl = licenseUrl;
        if (diplomaUrl != null) DiplomaCertificateUrl = diplomaUrl;
        if (identityUrl != null) IdentityDocumentUrl = identityUrl;
        if (additionalUrls != null) AdditionalCertificatesUrls = additionalUrls;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateProfessionalInfo(string licenseNumber, string professionalCollege, 
        string? collegeRegion, List<PsychologySpecialty> specialties, int yearsOfExperience,
        string? university = null, int? graduationYear = null, string? degree = null)
    {
        LicenseNumber = licenseNumber;
        ProfessionalCollege = professionalCollege;
        CollegeRegion = collegeRegion;
        Specialties = specialties;
        YearsOfExperience = yearsOfExperience;
        University = university;
        GraduationYear = graduationYear;
        Degree = degree;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateProfessionalProfile(string? professionalBio = null, bool? isAcceptingNewPatients = null,
        int? maxPatientsCapacity = null, List<string>? targetAudience = null, List<string>? languages = null,
        string? businessName = null, string? businessAddress = null)
    {
        if (professionalBio != null) ProfessionalBio = professionalBio;
        if (isAcceptingNewPatients.HasValue) IsAcceptingNewPatients = isAcceptingNewPatients.Value;
        if (maxPatientsCapacity.HasValue) MaxPatientsCapacity = maxPatientsCapacity.Value;
        if (targetAudience != null) TargetAudience = targetAudience;
        if (languages != null) Languages = languages;
        if (businessName != null) BusinessName = businessName;
        if (businessAddress != null) BusinessAddress = businessAddress;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementPatientsCount()
    {
        CurrentPatientsCount = (CurrentPatientsCount ?? 0) + 1;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DecrementPatientsCount()
    {
        if (CurrentPatientsCount > 0)
        {
            CurrentPatientsCount = CurrentPatientsCount - 1;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void UpdateDirectoryVisibility(bool isVisible)
    {
        IsProfileVisibleInDirectory = isVisible;
        UpdatedAt = DateTime.UtcNow;
    }

    public override void ValidateForCreation()
    {
        base.ValidateForCreation();
        
        if (string.IsNullOrWhiteSpace(LicenseNumber))
            throw new ArgumentException("License number is required for psychologists");

        if (string.IsNullOrWhiteSpace(ProfessionalCollege))
            throw new ArgumentException("Professional college is required for psychologists");

        if (Specialties == null || Specialties.Count == 0)
            throw new ArgumentException("At least one specialty is required for psychologists");

        if (YearsOfExperience < 0)
            throw new ArgumentException("Years of experience cannot be negative");
    }

    private static string GenerateRandomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}