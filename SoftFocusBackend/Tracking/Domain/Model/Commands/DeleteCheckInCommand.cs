namespace SoftFocusBackend.Tracking.Domain.Model.Commands;

public record DeleteCheckInCommand
{
    public string CheckInId { get; init; }
    public DateTime RequestedAt { get; init; }

    public DeleteCheckInCommand(string checkInId)
    {
        CheckInId = checkInId ?? throw new ArgumentNullException(nameof(checkInId));
        RequestedAt = DateTime.UtcNow;
    }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(CheckInId);
    }
}