using SoftFocusBackend.Users.Domain.Model.Aggregates;
using SoftFocusBackend.Users.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Users.Application.Internal.OutboundServices;

public interface IUserFacade
{
    Task<User?> GetUserByIdAsync(string userId);
    Task<User?> GetUserByEmailAsync(string email);
    Task<PsychologistUser?> GetPsychologistByInvitationCodeAsync(string invitationCode);
    Task<List<PsychologistUser>> GetVerifiedPsychologistsAsync();
    Task<(List<User> Users, int TotalCount)> GetAllUsersAsync(int page = 1, int pageSize = 20, 
        UserType? userType = null, bool? isActive = null, string? searchTerm = null);
    Task<int> GetTotalUsersCountAsync();
    Task<int> GetActiveUsersCountAsync();
    Task<bool> IsEmailAvailableAsync(string email, string? excludeUserId = null);
    Task<UserStats> GetUserStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    
    Task<User?> CreateUserAsync(string email, string password, string fullName, string userType, 
        string? professionalLicense = null, string[]? specialties = null);
    
    Task<User?> CreateOAuthUserAsync(string email, string fullName, string? profileImageUrl = null);
    Task<bool> UserExistsAsync(string userId);
    Task<string> GetUserEmailByIdAsync(string userId);
    Task<string> GetUserPhoneByIdAsync(string userId);
}

public record UserStats
{
    public int TotalUsers { get; init; }
    public int ActiveUsers { get; init; }
    public int PsychologistsCount { get; init; }
    public int VerifiedPsychologistsCount { get; init; }
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public int NewUsersThisPeriod { get; init; }
    public double ActiveUserPercentage { get; init; }
}