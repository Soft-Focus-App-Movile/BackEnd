using SoftFocusBackend.Users.Application.Internal.OutboundServices;
using SoftFocusBackend.Users.Domain.Model.Aggregates;
using SoftFocusBackend.Users.Domain.Model.Commands;
using SoftFocusBackend.Users.Domain.Model.ValueObjects;
using SoftFocusBackend.Users.Domain.Services;
using SoftFocusBackend.Users.Infrastructure.Persistence.MongoDB.Repositories;

namespace SoftFocusBackend.Users.Application.Internal.OutboundServices;

public class UserFacade : IUserFacade
{
    private readonly IUserRepository _userRepository;
    private readonly IPsychologistRepository _psychologistRepository;
    private readonly IUserCommandService _userCommandService;
    private readonly ILogger<UserFacade> _logger;

    public UserFacade(
        IUserRepository userRepository, 
        IPsychologistRepository psychologistRepository,
        IUserCommandService userCommandService,
        ILogger<UserFacade> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _psychologistRepository = psychologistRepository ?? throw new ArgumentNullException(nameof(psychologistRepository));
        _userCommandService = userCommandService ?? throw new ArgumentNullException(nameof(userCommandService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ---------------------- USUARIOS ----------------------
    public async Task<User?> GetUserByIdAsync(string userId)
    {
        try
        {
            _logger.LogDebug("Facade: Getting user by id: {UserId}", userId);
            return await _userRepository.FindByIdAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Facade: Error getting user by id: {UserId}", userId);
            return null;
        }
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        try
        {
            _logger.LogDebug("Facade: Getting user by email: {Email}", email);
            return await _userRepository.FindByEmailAsync(email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Facade: Error getting user by email: {Email}", email);
            return null;
        }
    }

    public async Task<bool> UserExistsAsync(string userId)
    {
        var user = await _userRepository.FindByIdAsync(userId);
        return user != null;
    }

    public async Task<string> GetUserEmailByIdAsync(string userId)
    {
        var user = await _userRepository.FindByIdAsync(userId);
        return user?.Email ?? string.Empty;
    }

    public async Task<string> GetUserPhoneByIdAsync(string userId)
    {
        var user = await _userRepository.FindByIdAsync(userId);
        return user?.Phone ?? string.Empty;
    }

    // ---------------------- PSICÓLOGOS ----------------------
    public async Task<PsychologistUser?> GetPsychologistByInvitationCodeAsync(string invitationCode)
    {
        try
        {
            _logger.LogDebug("Facade: Getting psychologist by invitation code: {Code}", invitationCode);
            return await _psychologistRepository.FindByInvitationCodeAsync(invitationCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Facade: Error getting psychologist by invitation code: {Code}", invitationCode);
            return null;
        }
    }

    public async Task<List<PsychologistUser>> GetVerifiedPsychologistsAsync()
    {
        try
        {
            _logger.LogDebug("Facade: Getting all verified psychologists");
            return await _psychologistRepository.FindVerifiedPsychologistsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Facade: Error getting verified psychologists");
            return new List<PsychologistUser>();
        }
    }

    // ---------------------- CONSULTAS GENERALES ----------------------
    public async Task<(List<User> Users, int TotalCount)> GetAllUsersAsync(int page = 1, int pageSize = 20, 
        UserType? userType = null, bool? isActive = null, string? searchTerm = null)
    {
        try
        {
            _logger.LogDebug("Facade: Getting all users - Page: {Page}, Size: {PageSize}", page, pageSize);
            return await _userRepository.FindAllUsersAsync(page, pageSize, userType, isActive, null, searchTerm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Facade: Error getting all users");
            return (new List<User>(), 0);
        }
    }

    public async Task<int> GetTotalUsersCountAsync()
    {
        try
        {
            _logger.LogDebug("Facade: Getting total users count");
            return await _userRepository.GetTotalUsersCountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Facade: Error getting total users count");
            return 0;
        }
    }

    public async Task<int> GetActiveUsersCountAsync()
    {
        try
        {
            _logger.LogDebug("Facade: Getting active users count");
            return await _userRepository.GetActiveUsersCountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Facade: Error getting active users count");
            return 0;
        }
    }

    public async Task<bool> IsEmailAvailableAsync(string email, string? excludeUserId = null)
    {
        try
        {
            _logger.LogDebug("Facade: Checking email availability: {Email}", email);
            var exists = await _userRepository.ExistsWithEmailAsync(email, excludeUserId);
            return !exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Facade: Error checking email availability: {Email}", email);
            return false;
        }
    }

    public async Task<UserStats> GetUserStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            _logger.LogDebug("Facade: Getting user statistics");

            var totalUsers = await _userRepository.GetTotalUsersCountAsync();
            var activeUsers = await _userRepository.GetActiveUsersCountAsync();
            var verifiedPsychologists = await _psychologistRepository.FindVerifiedPsychologistsAsync();

            var (allUsers, _) = await _userRepository.FindAllUsersAsync(1, int.MaxValue, UserType.Psychologist);
            var psychologistsCount = allUsers.Count;

            var fromDateValue = fromDate ?? DateTime.UtcNow.AddDays(-30);
            var toDateValue = toDate ?? DateTime.UtcNow;

            var (recentUsers, _) = await _userRepository.FindAllUsersAsync(1, int.MaxValue);
            var newUsersThisPeriod = recentUsers.Count(u => u.CreatedAt >= fromDateValue && u.CreatedAt <= toDateValue);

            var activePercentage = totalUsers > 0 ? (double)activeUsers / totalUsers * 100 : 0;

            return new UserStats
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                PsychologistsCount = psychologistsCount,
                VerifiedPsychologistsCount = verifiedPsychologists.Count,
                PeriodStart = fromDateValue,
                PeriodEnd = toDateValue,
                NewUsersThisPeriod = newUsersThisPeriod,
                ActiveUserPercentage = Math.Round(activePercentage, 2)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Facade: Error getting user statistics");
            return new UserStats
            {
                PeriodStart = fromDate ?? DateTime.UtcNow.AddDays(-30),
                PeriodEnd = toDate ?? DateTime.UtcNow
            };
        }
    }

    // ---------------------- CREACIÓN DE USUARIOS ----------------------
    public async Task<User?> CreateUserAsync(string email, string password, string fullName, string userType, 
        string? professionalLicense = null, string[]? specialties = null)
    {
        try
        {
            _logger.LogDebug("Facade: Creating user with email: {Email}", email);

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            if (!Enum.TryParse<UserType>(userType, out var userTypeEnum))
            {
                _logger.LogWarning("Facade: Invalid user type: {UserType}", userType);
                return null;
            }

            List<PsychologySpecialty>? specialtiesList = null;
            if (specialties != null && userTypeEnum == UserType.Psychologist)
            {
                specialtiesList = new List<PsychologySpecialty>();
                foreach (var specialty in specialties)
                {
                    if (Enum.TryParse<PsychologySpecialty>(specialty, out var specialtyEnum))
                    {
                        specialtiesList.Add(specialtyEnum);
                    }
                }
            }

            var command = new CreateUserCommand(email, passwordHash, fullName, userTypeEnum, professionalLicense, specialtiesList);
            return await _userCommandService.HandleCreateUserAsync(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Facade: Error creating user: {Email}", email);
            return null;
        }
    }

    public async Task<User?> CreateOAuthUserAsync(string email, string fullName, string? profileImageUrl = null)
    {
        try
        {
            _logger.LogDebug("Facade: Creating OAuth user with email: {Email}", email);

            var command = new CreateUserCommand(email, "[OAUTH_USER]", fullName, UserType.General);
            var user = await _userCommandService.HandleCreateUserAsync(command);

            if (user != null && !string.IsNullOrWhiteSpace(profileImageUrl))
            {
                user.SetProfileImageUrl(profileImageUrl);
                _userRepository.Update(user);
            }

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Facade: Error creating OAuth user: {Email}", email);
            return null;
        }
    }
}
