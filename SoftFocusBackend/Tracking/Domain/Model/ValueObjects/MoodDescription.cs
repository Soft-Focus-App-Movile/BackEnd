namespace SoftFocusBackend.Tracking.Domain.Model.ValueObjects;

public record MoodDescription
{
    public string Value { get; init; }

    public MoodDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Mood description cannot be null or empty.", nameof(description));

        if (description.Length > 500)
            throw new ArgumentException("Mood description cannot exceed 500 characters.", nameof(description));

        Value = description.Trim();
    }

    public static implicit operator string(MoodDescription description) => description.Value;
    public static implicit operator MoodDescription(string description) => new(description);
}