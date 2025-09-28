namespace SoftFocusBackend.Users.Domain.Model.Commands;

public record VerifyPsychologistCommand
{
    public string UserId { get; init; }
    public string VerifiedBy { get; init; }
    public bool IsApproved { get; init; }
    public string? VerificationNotes { get; init; }
    public DateTime RequestedAt { get; init; }

    public VerifyPsychologistCommand(string userId, string verifiedBy, bool isApproved,
        string? verificationNotes = null)
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        VerifiedBy = verifiedBy ?? throw new ArgumentNullException(nameof(verifiedBy));
        IsApproved = isApproved;
        VerificationNotes = verificationNotes?.Trim();
        RequestedAt = DateTime.UtcNow;
    }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(UserId) &&
               !string.IsNullOrWhiteSpace(VerifiedBy);
    }

    public string GetAuditString()
    {
        return $"UserId: {UserId} | VerifiedBy: {VerifiedBy} | IsApproved: {IsApproved} | RequestedAt: {RequestedAt:yyyy-MM-dd HH:mm:ss} UTC";
    }
}