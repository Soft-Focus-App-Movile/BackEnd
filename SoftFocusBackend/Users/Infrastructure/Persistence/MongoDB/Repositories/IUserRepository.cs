using SoftFocusBackend.Shared.Domain.Repositories;
using SoftFocusBackend.Users.Domain.Model.Aggregates;
using SoftFocusBackend.Users.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Users.Infrastructure.Persistence.MongoDB.Repositories;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> FindByEmailAsync(string email);
    Task<bool> ExistsWithEmailAsync(string email, string? excludeUserId = null);
    Task<(List<User> Users, int TotalCount)> FindAllUsersAsync(
        int page, int pageSize, UserType? userType = null, bool? isActive = null, 
        bool? isVerified = null, string? searchTerm = null, string? sortBy = null, 
        bool sortDescending = false);
    Task<List<User>> FindUsersByIdsAsync(List<string> userIds);
    Task<int> GetTotalUsersCountAsync();
    Task<int> GetActiveUsersCountAsync();
    Task UpdateLastLoginAsync(string userId, DateTime lastLogin);
}