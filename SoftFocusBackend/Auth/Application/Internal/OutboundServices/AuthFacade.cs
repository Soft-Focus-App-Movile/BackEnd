using SoftFocusBackend.Auth.Application.ACL.Services;
using SoftFocusBackend.Auth.Application.Internal.OutboundServices;

namespace SoftFocusBackend.Auth.Application.Internal.OutboundServices;

public class AuthFacade : IAuthFacade
{
    private readonly IUserContextService _userContextService;
    private readonly ILogger<AuthFacade> _logger;

    public AuthFacade(IUserContextService userContextService, ILogger<AuthFacade> logger)
    {
        _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> IsUserCurrentlyActiveAsync(string userId, TimeSpan? activityThreshold = null)
    {
        try
        {
            _logger.LogDebug("Checking if user {UserId} is currently active", userId);

            var threshold = activityThreshold ?? TimeSpan.FromHours(1);
            
            var user = await _userContextService.GetUserByIdAsync(userId);
            
            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", userId);
                return false;
            }

            if (!user.LastLogin.HasValue)
            {
                _logger.LogDebug("User {UserId} has never logged in", userId);
                return false;
            }

            var isActive = DateTime.UtcNow - user.LastLogin.Value <= threshold;
            
            _logger.LogDebug("User {UserId} activity status: {IsActive} (last login: {LastLogin})", 
                userId, isActive, user.LastLogin);
            
            return isActive;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking user activity for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<int> GetActiveUsersCountAsync(TimeSpan period)
    {
        _logger.LogInformation("GetActiveUsersCountAsync - Use UserFacade instead for user statistics");
        return 0;
    }

    public async Task<List<RecentLoginInfo>> GetRecentLoginsAsync(int take = 10)
    {
        _logger.LogInformation("GetRecentLoginsAsync - Use UserFacade.GetAllUsers() and filter by LastLogin instead");
        return new List<RecentLoginInfo>();
    }

    public async Task<AuthStatsInfo> GetAuthenticationStatsAsync(DateTime from, DateTime to)
    {
        _logger.LogInformation("GetAuthenticationStatsAsync - Use UserFacade for user-based statistics");
        return new AuthStatsInfo { PeriodStart = from, PeriodEnd = to };
    }

    public async Task<List<FailedLoginAttempt>> GetFailedLoginsAsync(DateTime from, int take = 50)
    {
        _logger.LogInformation("GetFailedLoginsAsync - Not implemented, will be added for security monitoring");
        return new List<FailedLoginAttempt>();
    }

    public async Task<LoginTrendsInfo> GetLoginTrendsAsync(int days = 30)
    {
        _logger.LogInformation("GetLoginTrendsAsync - Use UserFacade for login trend analysis");
        return new LoginTrendsInfo();
    }

    public async Task<List<InactiveUserInfo>> GetInactiveUsersAsync(TimeSpan inactivePeriod, int take = 20)
    {
        _logger.LogInformation("GetInactiveUsersAsync - Use UserFacade.GetAllUsers() and filter by LastLogin instead");
        return new List<InactiveUserInfo>();
    }
}