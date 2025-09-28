using SoftFocusBackend.Users.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Users.Domain.Model.Queries;

public record GetAllUsersQuery
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public UserType? UserType { get; init; }
    public bool? IsActive { get; init; }
    public bool? IsVerified { get; init; }
    public string? SearchTerm { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
    public DateTime RequestedAt { get; init; }
    public string? RequestedBy { get; init; }

    public GetAllUsersQuery(int page = 1, int pageSize = 20, UserType? userType = null,
        bool? isActive = null, bool? isVerified = null, string? searchTerm = null,
        string? sortBy = null, bool sortDescending = false, string? requestedBy = null)
    {
        Page = Math.Max(1, page);
        PageSize = Math.Clamp(pageSize, 1, 100);
        UserType = userType;
        IsActive = isActive;
        IsVerified = isVerified;
        SearchTerm = searchTerm?.Trim();
        SortBy = sortBy?.Trim();
        SortDescending = sortDescending;
        RequestedAt = DateTime.UtcNow;
        RequestedBy = requestedBy;
    }

    public bool IsValid()
    {
        return Page > 0 && PageSize > 0 && PageSize <= 100;
    }

    public int GetSkip() => (Page - 1) * PageSize;

    public string GetAuditString()
    {
        var parts = new List<string> 
        { 
            $"Page: {Page}", 
            $"PageSize: {PageSize}"
        };

        if (UserType.HasValue)
            parts.Add($"UserType: {UserType}");

        if (IsActive.HasValue)
            parts.Add($"IsActive: {IsActive}");

        if (!string.IsNullOrWhiteSpace(SearchTerm))
            parts.Add($"SearchTerm: {SearchTerm}");

        if (!string.IsNullOrWhiteSpace(RequestedBy))
            parts.Add($"RequestedBy: {RequestedBy}");

        parts.Add($"RequestedAt: {RequestedAt:yyyy-MM-dd HH:mm:ss} UTC");

        return string.Join(" | ", parts);
    }
}