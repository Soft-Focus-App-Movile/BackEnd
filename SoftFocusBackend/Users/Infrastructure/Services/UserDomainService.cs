using SoftFocusBackend.Users.Domain.Model.Aggregates;
using SoftFocusBackend.Users.Domain.Model.ValueObjects;
using SoftFocusBackend.Users.Domain.Services;
using SoftFocusBackend.Users.Infrastructure.Persistence.MongoDB.Repositories;
using BCrypt.Net;

namespace SoftFocusBackend.Users.Infrastructure.Services;

public class UserDomainService : IUserDomainService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserDomainService> _logger;

    public UserDomainService(IUserRepository userRepository, ILogger<UserDomainService> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> IsEmailUniqueAsync(string email, string? excludeUserId = null)
    {
        try
        {
            var exists = await _userRepository.ExistsWithEmailAsync(email, excludeUserId);
            return !exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email uniqueness: {Email}", email);
            return false;
        }
    }

    public async Task<bool> ValidatePasswordStrengthAsync(string password)
    {
        await Task.CompletedTask;
        
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return false;

        var hasUpper = password.Any(char.IsUpper);
        var hasLower = password.Any(char.IsLower);
        var hasDigit = password.Any(char.IsDigit);
        var hasSpecial = password.Any(c => "@$!%*?&".Contains(c));

        return hasUpper && hasLower && hasDigit && hasSpecial;
    }

    public async Task<UserEmail> NormalizeEmailAsync(string email)
    {
        await Task.CompletedTask;
        return new UserEmail(email);
    }

    public async Task<bool> CanUserBeDeletedAsync(string userId)
    {
        try
        {
            var user = await _userRepository.FindByIdAsync(userId);
            if (user == null) return false;

            if (user.IsPsychologist())
            {
                var psychologist = user as PsychologistUser;
                if (psychologist?.CurrentPatientsCount > 0)
                {
                    _logger.LogWarning("Cannot delete psychologist with active patients: {UserId}", userId);
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user can be deleted: {UserId}", userId);
            return false;
        }
    }

    public async Task<User> CreateUserAsync(string email, string passwordHash, string fullName, UserType userType)
    {
        await Task.CompletedTask;
        
        var user = new User
        {
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            FullName = fullName,
            UserType = userType,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        user.ValidateForCreation();
        return user;
    }

    public async Task<PsychologistUser> CreatePsychologistAsync(string email, string passwordHash, string fullName, 
        string licenseNumber, string professionalCollege, List<PsychologySpecialty> specialties, 
        int yearsOfExperience)
    {
        await Task.CompletedTask;
        
        var psychologist = new PsychologistUser
        {
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            FullName = fullName,
            UserType = UserType.Psychologist,
            LicenseNumber = licenseNumber,
            ProfessionalCollege = professionalCollege,
            Specialties = specialties,
            YearsOfExperience = yearsOfExperience,
            IsActive = true,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        psychologist.ValidateForCreation();
        return psychologist;
    }
}