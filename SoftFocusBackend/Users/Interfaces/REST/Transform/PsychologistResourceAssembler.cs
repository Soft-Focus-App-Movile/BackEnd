using SoftFocusBackend.Users.Domain.Model.Aggregates;
using SoftFocusBackend.Users.Domain.Model.Commands;
using SoftFocusBackend.Users.Domain.Model.ValueObjects;
using SoftFocusBackend.Users.Interfaces.REST.Resources;

namespace SoftFocusBackend.Users.Interfaces.REST.Transform;

public static class PsychologistResourceAssembler
{
    public static PsychologistVerificationResource ToVerificationResource(PsychologistUser psychologist)
    {
        return new PsychologistVerificationResource
        {
            LicenseNumber = psychologist.LicenseNumber,
            ProfessionalCollege = psychologist.ProfessionalCollege,
            CollegeRegion = psychologist.CollegeRegion,
            Specialties = psychologist.Specialties,
            YearsOfExperience = psychologist.YearsOfExperience,
            University = psychologist.University,
            GraduationYear = psychologist.GraduationYear,
            Degree = psychologist.Degree,
            LicenseDocumentUrl = psychologist.LicenseDocumentUrl,
            DiplomaCertificateUrl = psychologist.DiplomaCertificateUrl,
            IdentityDocumentUrl = psychologist.IdentityDocumentUrl,
            AdditionalCertificatesUrls = psychologist.AdditionalCertificatesUrls,
            IsVerified = psychologist.IsVerified,
            VerificationDate = psychologist.VerificationDate,
            VerifiedBy = psychologist.VerifiedBy,
            VerificationNotes = psychologist.VerificationNotes
        };
    }

    public static UpdatePsychologistVerificationCommand ToUpdateVerificationCommand(
        PsychologistVerificationResource resource,
        string userId,
        string? licenseDocumentUrl = null,
        string? diplomaCertificateUrl = null,
        string? identityDocumentUrl = null,
        List<string>? additionalCertificatesUrls = null)
    {
        return new UpdatePsychologistVerificationCommand(
            userId: userId,
            licenseNumber: resource.LicenseNumber,
            professionalCollege: resource.ProfessionalCollege,
            specialties: resource.Specialties,
            yearsOfExperience: resource.YearsOfExperience,
            collegeRegion: resource.CollegeRegion,
            university: resource.University,
            graduationYear: resource.GraduationYear,
            degree: resource.Degree,
            licenseDocumentUrl: licenseDocumentUrl ?? resource.LicenseDocumentUrl,
            diplomaCertificateUrl: diplomaCertificateUrl ?? resource.DiplomaCertificateUrl,
            identityDocumentUrl: identityDocumentUrl ?? resource.IdentityDocumentUrl,
            additionalCertificatesUrls: additionalCertificatesUrls ?? resource.AdditionalCertificatesUrls
        );
    }

    public static PsychologistProfessionalResource ToProfessionalResource(PsychologistUser psychologist)
    {
        return new PsychologistProfessionalResource
        {
            ProfessionalBio = psychologist.ProfessionalBio,
            IsAcceptingNewPatients = psychologist.IsAcceptingNewPatients,
            MaxPatientsCapacity = psychologist.MaxPatientsCapacity,
            TargetAudience = psychologist.TargetAudience,
            Languages = psychologist.Languages,
            BusinessName = psychologist.BusinessName,
            BusinessAddress = psychologist.BusinessAddress,
            BankAccount = psychologist.BankAccount,
            PaymentMethods = psychologist.PaymentMethods,
            IsProfileVisibleInDirectory = psychologist.IsProfileVisibleInDirectory,
            AllowsDirectMessages = psychologist.AllowsDirectMessages
        };
    }

    public static UpdateProfessionalProfileCommand ToUpdateProfessionalCommand(
        PsychologistProfessionalResource resource, string userId)
    {
        return new UpdateProfessionalProfileCommand(
            userId: userId,
            professionalBio: resource.ProfessionalBio,
            isAcceptingNewPatients: resource.IsAcceptingNewPatients,
            maxPatientsCapacity: resource.MaxPatientsCapacity,
            targetAudience: resource.TargetAudience,
            languages: resource.Languages,
            businessName: resource.BusinessName,
            businessAddress: resource.BusinessAddress,
            bankAccount: resource.BankAccount,
            paymentMethods: resource.PaymentMethods,
            isProfileVisibleInDirectory: resource.IsProfileVisibleInDirectory,
            allowsDirectMessages: resource.AllowsDirectMessages
        );
    }

    public static PsychologistStatsResource ToStatsResource(PsychologistStats stats)
    {
        return new PsychologistStatsResource
        {
            ConnectedPatientsCount = stats.ConnectedPatientsCount,
            TotalCheckInsReceived = stats.TotalCheckInsReceived,
            CrisisAlertsHandled = stats.CrisisAlertsHandled,
            AverageResponseTime = stats.GetFormattedResponseTime(),
            IsAcceptingNewPatients = stats.IsAcceptingNewPatients,
            LastActivityDate = stats.LastActivityDate,
            JoinedDate = stats.JoinedDate,
            AverageRating = stats.AverageRating,
            TotalReviews = stats.TotalReviews,
            ExperienceLevel = stats.GetExperienceLevel(),
            StatsGeneratedAt = DateTime.UtcNow
        };
    }

    public static PsychologistDirectoryResource ToDirectoryResource(PsychologistUser psychologist)
    {
        return new PsychologistDirectoryResource
        {
            Id = psychologist.Id,
            FullName = psychologist.FullName,
            ProfileImageUrl = psychologist.ProfileImageUrl,
            ProfessionalBio = psychologist.ProfessionalBio,
            Specialties = psychologist.Specialties,
            YearsOfExperience = psychologist.YearsOfExperience,
            City = psychologist.City,
            Languages = psychologist.Languages,
            IsAcceptingNewPatients = psychologist.IsAcceptingNewPatients,
            AverageRating = psychologist.AverageRating,
            TotalReviews = psychologist.TotalReviews ?? 0,
            AllowsDirectMessages = psychologist.AllowsDirectMessages,
            TargetAudience = psychologist.TargetAudience
        };
    }

    public static PsychologistCompleteProfileResource ToCompleteProfileResource(PsychologistUser psychologist)
    {
        return new PsychologistCompleteProfileResource
        {
            // Basic User Data
            Id = psychologist.Id,
            Email = psychologist.Email,
            FullName = psychologist.FullName,
            FirstName = psychologist.FirstName,
            LastName = psychologist.LastName,
            UserType = psychologist.UserType.ToString(),
            DateOfBirth = psychologist.DateOfBirth,
            Gender = psychologist.Gender,
            Phone = psychologist.Phone,
            ProfileImageUrl = psychologist.ProfileImageUrl,
            Bio = psychologist.Bio,
            Country = psychologist.Country,
            City = psychologist.City,
            Interests = psychologist.Interests,
            MentalHealthGoals = psychologist.MentalHealthGoals,
            EmailNotifications = psychologist.EmailNotifications,
            PushNotifications = psychologist.PushNotifications,
            IsProfilePublic = psychologist.IsProfilePublic,
            IsActive = psychologist.IsActive,
            LastLogin = psychologist.LastLogin,
            CreatedAt = psychologist.CreatedAt,
            UpdatedAt = psychologist.UpdatedAt,

            // Verification Data
            LicenseNumber = psychologist.LicenseNumber,
            ProfessionalCollege = psychologist.ProfessionalCollege,
            CollegeRegion = psychologist.CollegeRegion,
            Specialties = psychologist.Specialties,
            YearsOfExperience = psychologist.YearsOfExperience,
            University = psychologist.University,
            GraduationYear = psychologist.GraduationYear,
            Degree = psychologist.Degree,
            LicenseDocumentUrl = psychologist.LicenseDocumentUrl,
            DiplomaCertificateUrl = psychologist.DiplomaCertificateUrl,
            IdentityDocumentUrl = psychologist.IdentityDocumentUrl,
            AdditionalCertificatesUrls = psychologist.AdditionalCertificatesUrls,
            IsVerified = psychologist.IsVerified,
            VerificationDate = psychologist.VerificationDate,
            VerifiedBy = psychologist.VerifiedBy,
            VerificationNotes = psychologist.VerificationNotes,

            // Professional Data
            ProfessionalBio = psychologist.ProfessionalBio,
            IsAcceptingNewPatients = psychologist.IsAcceptingNewPatients,
            MaxPatientsCapacity = psychologist.MaxPatientsCapacity,
            CurrentPatientsCount = psychologist.CurrentPatientsCount,
            TargetAudience = psychologist.TargetAudience,
            Languages = psychologist.Languages,
            BusinessName = psychologist.BusinessName,
            BusinessAddress = psychologist.BusinessAddress,
            BankAccount = psychologist.BankAccount,
            PaymentMethods = psychologist.PaymentMethods,
            Currency = psychologist.Currency,
            IsProfileVisibleInDirectory = psychologist.IsProfileVisibleInDirectory,
            AllowsDirectMessages = psychologist.AllowsDirectMessages,
            AverageRating = psychologist.AverageRating,
            TotalReviews = psychologist.TotalReviews
        };
    }

    public static object ToErrorResponse(string message, string? details = null)
    {
        return new
        {
            error = true,
            message,
            details,
            timestamp = DateTime.UtcNow
        };
    }
}