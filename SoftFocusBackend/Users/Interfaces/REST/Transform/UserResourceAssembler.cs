using SoftFocusBackend.Users.Domain.Model.Aggregates;
using SoftFocusBackend.Users.Domain.Model.Commands;
using SoftFocusBackend.Users.Interfaces.REST.Resources;

namespace SoftFocusBackend.Users.Interfaces.REST.Transform;

public static class UserResourceAssembler
{
    public static UserProfileResource ToProfileResource(User user)
    {
        return new UserProfileResource
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            UserType = user.UserType,
            DateOfBirth = user.DateOfBirth,
            Gender = user.Gender,
            Phone = user.Phone,
            ProfileImageUrl = user.ProfileImageUrl,
            Bio = user.Bio,
            Country = user.Country,
            City = user.City,
            Interests = user.Interests,
            MentalHealthGoals = user.MentalHealthGoals,
            EmailNotifications = user.EmailNotifications,
            PushNotifications = user.PushNotifications,
            IsProfilePublic = user.IsProfilePublic,
            IsActive = user.IsActive,
            LastLogin = user.LastLogin,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    public static UpdateUserProfileCommand ToUpdateCommandWithImage(UpdateUserProfileResource resource, string userId, byte[]? imageBytes = null, string? imageFileName = null)
    {
        return new UpdateUserProfileCommand(
            userId: userId,
            fullName: resource.FullName,
            firstName: resource.FirstName,
            lastName: resource.LastName,
            dateOfBirth: resource.DateOfBirth,
            gender: resource.Gender,
            phone: resource.Phone,
            bio: resource.Bio,
            country: resource.Country,
            city: resource.City,
            interests: resource.Interests,
            mentalHealthGoals: resource.MentalHealthGoals,
            emailNotifications: resource.EmailNotifications,
            pushNotifications: resource.PushNotifications,
            isProfilePublic: resource.IsProfilePublic,
            profileImageUrl: resource.ProfileImageUrl,
            profileImageBytes: imageBytes,
            profileImageFileName: imageFileName
        );
    }

    public static object ToAdminUserResource(User user)
    {
        return new
        {
            id = user.Id,
            email = user.Email,
            fullName = user.FullName,
            userType = user.UserType.ToString(),
            isActive = user.IsActive,
            lastLogin = user.LastLogin,
            createdAt = user.CreatedAt,
            isVerified = user is PsychologistUser psychologist ? psychologist.IsVerified : (bool?)null
        };
    }

    public static object ToAdminUserDetailResource(User user)
    {
        var baseResource = new
        {
            id = user.Id,
            email = user.Email,
            fullName = user.FullName,
            firstName = user.FirstName,
            lastName = user.LastName,
            userType = user.UserType.ToString(),
            dateOfBirth = user.DateOfBirth,
            gender = user.Gender,
            phone = user.Phone,
            profileImageUrl = user.ProfileImageUrl,
            bio = user.Bio,
            country = user.Country,
            city = user.City,
            interests = user.Interests,
            mentalHealthGoals = user.MentalHealthGoals,
            emailNotifications = user.EmailNotifications,
            pushNotifications = user.PushNotifications,
            isProfilePublic = user.IsProfilePublic,
            isActive = user.IsActive,
            lastLogin = user.LastLogin,
            createdAt = user.CreatedAt,
            updatedAt = user.UpdatedAt
        };

        if (user is PsychologistUser psychologist)
        {
            return new
            {
                user = baseResource,
                psychologistData = new
                {
                    licenseNumber = psychologist.LicenseNumber,
                    professionalCollege = psychologist.ProfessionalCollege,
                    specialties = psychologist.Specialties,
                    yearsOfExperience = psychologist.YearsOfExperience,
                    isVerified = psychologist.IsVerified,
                    verificationDate = psychologist.VerificationDate,
                    verifiedBy = psychologist.VerifiedBy,
                    currentPatientsCount = psychologist.CurrentPatientsCount,
                    isAcceptingNewPatients = psychologist.IsAcceptingNewPatients
                }
            };
        }

        return new { user = baseResource };
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