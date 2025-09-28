namespace SoftFocusBackend.Users.Domain.Model.Queries;

public record GetUserByEmailQuery
{
    public string Email { get; init; }
    public bool IncludePsychologistData { get; init; }
    public DateTime RequestedAt { get; init; }
    public string? RequestedBy { get; init; }

    public GetUserByEmailQuery(string email, bool includePsychologistData = false, string? requestedBy = null)
    {
        Email = email?.Trim().ToLowerInvariant() ?? throw new ArgumentNullException(nameof(email));
        IncludePsychologistData = includePsychologistData;
        RequestedAt = DateTime.UtcNow;
        RequestedBy = requestedBy;
    }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Email) && Email.Contains('@');
    }

    public string GetAuditString()
    {
        var parts = new List<string> { $"Email: {Email}", $"IncludePsychologistData: {IncludePsychologistData}" };

        if (!string.IsNullOrWhiteSpace(RequestedBy))
            parts.Add($"RequestedBy: {RequestedBy}");

        parts.Add($"RequestedAt: {RequestedAt:yyyy-MM-dd HH:mm:ss} UTC");

        return string.Join(" | ", parts);
    }
}