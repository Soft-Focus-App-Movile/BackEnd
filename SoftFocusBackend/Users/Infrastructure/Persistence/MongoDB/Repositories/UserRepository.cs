using MongoDB.Driver;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Context;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Repositories;
using SoftFocusBackend.Users.Domain.Model.Aggregates;
using SoftFocusBackend.Users.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Users.Infrastructure.Persistence.MongoDB.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(MongoDbContext context) : base(context, "users")
    {
    }

    public async Task<User?> FindByEmailAsync(string email)
    {
        var normalizedEmail = email.ToLowerInvariant();
        return await Collection.Find(u => u.Email == normalizedEmail).FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsWithEmailAsync(string email, string? excludeUserId = null)
    {
        var normalizedEmail = email.ToLowerInvariant();
        var filter = Builders<User>.Filter.Eq(u => u.Email, normalizedEmail);
        
        if (!string.IsNullOrWhiteSpace(excludeUserId))
        {
            filter = Builders<User>.Filter.And(filter, 
                Builders<User>.Filter.Ne(u => u.Id, excludeUserId));
        }
        
        return await Collection.Find(filter).AnyAsync();
    }

    public async Task<(List<User> Users, int TotalCount)> FindAllUsersAsync(
        int page, int pageSize, UserType? userType = null, bool? isActive = null,
        bool? isVerified = null, string? searchTerm = null, string? sortBy = null,
        bool sortDescending = false)
    {
        var filterBuilder = Builders<User>.Filter;
        var filter = filterBuilder.Empty;

        if (userType.HasValue)
        {
            filter = filterBuilder.And(filter, filterBuilder.Eq(u => u.UserType, userType.Value));
        }

        if (isActive.HasValue)
        {
            filter = filterBuilder.And(filter, filterBuilder.Eq(u => u.IsActive, isActive.Value));
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchFilter = filterBuilder.Or(
                filterBuilder.Where(u => u.FullName.ToLower().Contains(searchTerm.ToLower())),
                filterBuilder.Where(u => u.Email.ToLower().Contains(searchTerm.ToLower()))
            );
            filter = filterBuilder.And(filter, searchFilter);
        }

        var totalCount = await Collection.CountDocumentsAsync(filter);

        var sortDefinition = CreateSortDefinition(sortBy, sortDescending);
        var skip = (page - 1) * pageSize;

        var users = await Collection.Find(filter)
            .Sort(sortDefinition)
            .Skip(skip)
            .Limit(pageSize)
            .ToListAsync();

        return (users, (int)totalCount);
    }

    public async Task<List<User>> FindUsersByIdsAsync(List<string> userIds)
    {
        var filter = Builders<User>.Filter.In(u => u.Id, userIds);
        return await Collection.Find(filter).ToListAsync();
    }

    public async Task<int> GetTotalUsersCountAsync()
    {
        return (int)await Collection.CountDocumentsAsync(Builders<User>.Filter.Empty);
    }

    public async Task<int> GetActiveUsersCountAsync()
    {
        var filter = Builders<User>.Filter.Eq(u => u.IsActive, true);
        return (int)await Collection.CountDocumentsAsync(filter);
    }

    public async Task UpdateLastLoginAsync(string userId, DateTime lastLogin)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
        var update = Builders<User>.Update
            .Set(u => u.LastLogin, lastLogin)
            .Set(u => u.UpdatedAt, DateTime.UtcNow);
            
        await Collection.UpdateOneAsync(filter, update);
    }

    private static SortDefinition<User> CreateSortDefinition(string? sortBy, bool sortDescending)
    {
        var sortBuilder = Builders<User>.Sort;
        
        var sortField = sortBy?.ToLowerInvariant() switch
        {
            "fullname" => sortDescending ? sortBuilder.Descending(u => u.FullName) : sortBuilder.Ascending(u => u.FullName),
            "email" => sortDescending ? sortBuilder.Descending(u => u.Email) : sortBuilder.Ascending(u => u.Email),
            "usertype" => sortDescending ? sortBuilder.Descending(u => u.UserType) : sortBuilder.Ascending(u => u.UserType),
            "createdat" => sortDescending ? sortBuilder.Descending(u => u.CreatedAt) : sortBuilder.Ascending(u => u.CreatedAt),
            "lastlogin" => sortDescending ? sortBuilder.Descending(u => u.LastLogin) : sortBuilder.Ascending(u => u.LastLogin),
            _ => sortDescending ? sortBuilder.Descending(u => u.CreatedAt) : sortBuilder.Ascending(u => u.CreatedAt)
        };

        return sortField;
    }
}