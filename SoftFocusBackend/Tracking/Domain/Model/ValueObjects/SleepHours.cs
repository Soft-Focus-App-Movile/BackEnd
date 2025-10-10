namespace SoftFocusBackend.Tracking.Domain.Model.ValueObjects;

public record SleepHours
{
    public decimal Value { get; init; }

    public SleepHours(decimal hours)
    {
        if (hours < 0 || hours > 24)
            throw new ArgumentException("Sleep hours must be between 0 and 24.", nameof(hours));

        Value = hours;
    }

    public static implicit operator decimal(SleepHours hours) => hours.Value;
    public static implicit operator SleepHours(decimal hours) => new(hours);
}