namespace SoftFocusBackend.Tracking.Domain.Model.ValueObjects;

public record EnergyLevel
{
    public int Value { get; init; }

    public EnergyLevel(int level)
    {
        if (level < 1 || level > 10)
            throw new ArgumentException("Energy level must be between 1 and 10.", nameof(level));

        Value = level;
    }

    public static implicit operator int(EnergyLevel level) => level.Value;
    public static implicit operator EnergyLevel(int level) => new(level);
}