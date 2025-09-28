namespace SoftFocusBackend.Users.Domain.Model.Commands;

public record RegenerateInvitationCodeCommand
{
    public string UserId { get; init; }
    public string Reason { get; init; }
    public DateTime RequestedAt { get; init; }
    public string? RequestedBy { get; init; }

    public RegenerateInvitationCodeCommand(string userId, string reason = "Manual regeneration requested",
        string? requestedBy = null)
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        RequestedAt = DateTime.UtcNow;
        RequestedBy = requestedBy;
    }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(UserId) &&
               !string.IsNullOrWhiteSpace(Reason);
    }

    public string GetAuditString()
    {
        var parts = new List<string> { $"UserId: {UserId}", $"Reason: {Reason}" };

        if (!string.IsNullOrWhiteSpace(RequestedBy))
            parts.Add($"RequestedBy: {RequestedBy}");

        parts.Add($"RequestedAt: {RequestedAt:yyyy-MM-dd HH:mm:ss} UTC");

        return string.Join(" | ", parts);
    }
}