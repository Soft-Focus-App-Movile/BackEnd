namespace SoftFocusBackend.Tracking.Domain.Model.Queries;

public record GetCheckInByIdQuery
{
    public string CheckInId { get; init; }
    public DateTime RequestedAt { get; init; }

    public GetCheckInByIdQuery(string checkInId)
    {
        CheckInId = checkInId ?? throw new ArgumentNullException(nameof(checkInId));
        RequestedAt = DateTime.UtcNow;
    }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(CheckInId);
    }
}