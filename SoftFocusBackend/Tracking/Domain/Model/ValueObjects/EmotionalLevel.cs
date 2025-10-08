namespace SoftFocusBackend.Tracking.Domain.Model.ValueObjects;

public record EmotionalLevel
{
    public int Value { get; init; }

    public EmotionalLevel(int level)
    {
        if (level < 1 || level > 10)
            throw new ArgumentException("Emotional level must be between 1 and 10.", nameof(level));

        Value = level;
    }

    public static implicit operator int(EmotionalLevel level) => level.Value;
    public static implicit operator EmotionalLevel(int level) => new(level);
}