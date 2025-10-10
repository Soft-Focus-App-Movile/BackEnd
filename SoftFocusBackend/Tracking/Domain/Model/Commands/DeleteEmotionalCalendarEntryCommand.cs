namespace SoftFocusBackend.Tracking.Domain.Model.Commands;

public record DeleteEmotionalCalendarEntryCommand
{
    public string EntryId { get; init; }
    public DateTime RequestedAt { get; init; }

    public DeleteEmotionalCalendarEntryCommand(string entryId)
    {
        EntryId = entryId ?? throw new ArgumentNullException(nameof(entryId));
        RequestedAt = DateTime.UtcNow;
    }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(EntryId);
    }
}