namespace SoftFocusBackend.Users.Domain.Model.Queries;

public record GetUserByIdQuery
{
    public string UserId { get; init; }
    public bool IncludePsychologistData { get; init; }
    public DateTime RequestedAt { get; init; }
    public string? RequestedBy { get; init; }

    public GetUserByIdQuery(string userId, bool includePsychologistData = false, string? requestedBy = null)
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        IncludePsychologistData = includePsychologistData;
        RequestedAt = DateTime.UtcNow;
        RequestedBy = requestedBy;
    }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(UserId);
    }

    public string GetAuditString()
    {
        var parts = new List<string> { $"UserId: {UserId}", $"IncludePsychologistData: {IncludePsychologistData}" };

        if (!string.IsNullOrWhiteSpace(RequestedBy))
            parts.Add($"RequestedBy: {RequestedBy}");

        parts.Add($"RequestedAt: {RequestedAt:yyyy-MM-dd HH:mm:ss} UTC");

        return string.Join(" | ", parts);
    }
}