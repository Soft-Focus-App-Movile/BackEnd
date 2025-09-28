using SoftFocusBackend.Auth.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Auth.Application.Internal.OutboundServices;

public interface IAuthFacade
{
    Task<int> GetActiveUsersCountAsync(TimeSpan period);
    Task<List<RecentLoginInfo>> GetRecentLoginsAsync(int take = 10);
    Task<AuthStatsInfo> GetAuthenticationStatsAsync(DateTime from, DateTime to);
    Task<List<FailedLoginAttempt>> GetFailedLoginsAsync(DateTime from, int take = 50);
    Task<bool> IsUserCurrentlyActiveAsync(string userId, TimeSpan? activityThreshold = null);
    Task<LoginTrendsInfo> GetLoginTrendsAsync(int days = 30);
    Task<List<InactiveUserInfo>> GetInactiveUsersAsync(TimeSpan inactivePeriod, int take = 20);
}

public record RecentLoginInfo
{
    public string UserId { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public DateTime? LastLogin { get; init; }
    public string? IpAddress { get; init; }
}

public record AuthStatsInfo
{
    public int TotalLogins { get; init; }
    public int UniqueUsers { get; init; }
    public int FailedAttempts { get; init; }
    public double AverageLoginsPerDay { get; init; }
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
}

public record FailedLoginAttempt
{
    public string Email { get; init; } = string.Empty;
    public DateTime AttemptTime { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public record LoginTrendsInfo
{
    public List<DailyLoginCount> DailyCounts { get; init; } = new();
    public double AverageLoginsPerDay { get; init; }
    public int TotalLogins { get; init; }
    public int TrendDirection { get; init; }
}

public record DailyLoginCount
{
    public DateTime Date { get; init; }
    public int LoginCount { get; init; }
    public int UniqueUsers { get; init; }
}

public record InactiveUserInfo
{
    public string UserId { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public DateTime? LastLogin { get; init; }
    public int DaysSinceLastLogin { get; init; }
}