namespace SoftFocusBackend.Tracking.Domain.Model.ValueObjects;

public record CheckInNotes
{
    public string Value { get; init; }

    public CheckInNotes(string notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
            throw new ArgumentException("Check-in notes cannot be null or empty.", nameof(notes));

        if (notes.Length > 1000)
            throw new ArgumentException("Check-in notes cannot exceed 1000 characters.", nameof(notes));

        Value = notes.Trim();
    }

    public static implicit operator string(CheckInNotes notes) => notes.Value;
    public static implicit operator CheckInNotes(string notes) => new(notes);
}