using SoftFocusBackend.Users.Domain.Model.Aggregates;
using SoftFocusBackend.Users.Domain.Model.Queries;
using SoftFocusBackend.Users.Domain.Services;
using SoftFocusBackend.Users.Infrastructure.Persistence.MongoDB.Repositories;

namespace SoftFocusBackend.Users.Application.Internal.QueryServices;

public class UserQueryService : IUserQueryService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserQueryService> _logger;

    public UserQueryService(
        IUserRepository userRepository,
        ILogger<UserQueryService> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<User?> HandleGetUserByIdAsync(GetUserByIdQuery query)
    {
        try
        {
            _logger.LogDebug("Processing get user by id query: {AuditInfo}", query.GetAuditString());

            if (!query.IsValid())
            {
                _logger.LogWarning("Invalid get user by id query for user: {UserId}", query.UserId);
                return null;
            }

            var user = await _userRepository.FindByIdAsync(query.UserId);

            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", query.UserId);
                return null;
            }

            _logger.LogDebug("User retrieved successfully: {UserId} - {Email}", user.Id, user.Email);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by id: {UserId}", query.UserId);
            return null;
        }
    }

    public async Task<User?> HandleGetUserByEmailAsync(GetUserByEmailQuery query)
    {
        try
        {
            _logger.LogDebug("Processing get user by email query: {AuditInfo}", query.GetAuditString());

            if (!query.IsValid())
            {
                _logger.LogWarning("Invalid get user by email query for email: {Email}", query.Email);
                return null;
            }

            var user = await _userRepository.FindByEmailAsync(query.Email);

            if (user == null)
            {
                _logger.LogDebug("User not found for email: {Email}", query.Email);
                return null;
            }

            _logger.LogDebug("User retrieved successfully by email: {UserId} - {Email}", user.Id, user.Email);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user by email: {Email}", query.Email);
            return null;
        }
    }

    public async Task<(List<User> Users, int TotalCount)> HandleGetAllUsersAsync(GetAllUsersQuery query)
    {
        try
        {
            _logger.LogDebug("Processing get all users query: {AuditInfo}", query.GetAuditString());

            if (!query.IsValid())
            {
                _logger.LogWarning("Invalid get all users query");
                return (new List<User>(), 0);
            }

            var (users, totalCount) = await _userRepository.FindAllUsersAsync(
                query.Page,
                query.PageSize,
                query.UserType,
                query.IsActive,
                query.IsVerified,
                query.SearchTerm,
                query.SortBy,
                query.SortDescending);

            _logger.LogDebug("Users retrieved successfully: {Count} users, {TotalCount} total", users.Count, totalCount);
            return (users, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all users");
            return (new List<User>(), 0);
        }
    }
}

