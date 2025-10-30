using SoftFocusBackend.Users.Domain.Model.Aggregates;
using SoftFocusBackend.Users.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Users.Domain.Services;

public interface IUserDomainService
{
    Task<bool> IsEmailUniqueAsync(string email, string? excludeUserId = null);
    Task<bool> ValidatePasswordStrengthAsync(string password);
    Task<UserEmail> NormalizeEmailAsync(string email);
    Task<bool> CanUserBeDeletedAsync(string userId);
    Task<User> CreateUserAsync(string email, string passwordHash, string fullName, UserType userType);
    Task<PsychologistUser> CreatePsychologistAsync(string email, string passwordHash, string fullName,
        string licenseNumber, string professionalCollege, List<PsychologySpecialty> specialties,
        int yearsOfExperience, string? collegiateRegion = null, string? university = null,
        int? graduationYear = null);
}