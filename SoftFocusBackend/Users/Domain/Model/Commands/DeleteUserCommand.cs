namespace SoftFocusBackend.Users.Domain.Model.Commands;

public record DeleteUserCommand
{
    public string UserId { get; init; }
    public string Reason { get; init; }
    public bool HardDelete { get; init; }
    public DateTime RequestedAt { get; init; }
    public string? RequestedBy { get; init; }

    public DeleteUserCommand(string userId, string reason = "User requested deletion", 
        bool hardDelete = false, string? requestedBy = null)
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        HardDelete = hardDelete;
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
        var parts = new List<string> 
        { 
            $"UserId: {UserId}", 
            $"Reason: {Reason}",
            $"HardDelete: {HardDelete}"
        };

        if (!string.IsNullOrWhiteSpace(RequestedBy))
            parts.Add($"RequestedBy: {RequestedBy}");

        parts.Add($"RequestedAt: {RequestedAt:yyyy-MM-dd HH:mm:ss} UTC");

        return string.Join(" | ", parts);
    }
}