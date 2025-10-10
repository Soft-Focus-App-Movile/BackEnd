namespace SoftFocusBackend.Tracking.Domain.Model.ValueObjects;

public record EmotionalEmoji
{
    public string Value { get; init; }

    public EmotionalEmoji(string emoji)
    {
        if (string.IsNullOrWhiteSpace(emoji))
            throw new ArgumentException("Emotional emoji cannot be null or empty.", nameof(emoji));

        if (emoji.Length > 10)
            throw new ArgumentException("Emotional emoji cannot exceed 10 characters.", nameof(emoji));

        Value = emoji.Trim();
    }

    public static implicit operator string(EmotionalEmoji emoji) => emoji.Value;
    public static implicit operator EmotionalEmoji(string emoji) => new(emoji);
}