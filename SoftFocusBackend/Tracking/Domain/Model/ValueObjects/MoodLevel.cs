namespace SoftFocusBackend.Tracking.Domain.Model.ValueObjects;

public record MoodLevel
{
    public int Value { get; init; }

    public MoodLevel(int level)
    {
        if (level < 1 || level > 10)
            throw new ArgumentException("Mood level must be between 1 and 10.", nameof(level));

        Value = level;
    }

    public static implicit operator int(MoodLevel level) => level.Value;
    public static implicit operator MoodLevel(int level) => new(level);
}