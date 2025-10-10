namespace SoftFocusBackend.Tracking.Domain.Model.ValueObjects;

public record TrackingDate
{
    public DateTime Value { get; init; }

    public TrackingDate(DateTime date)
    {
        if (date > DateTime.UtcNow.Date.AddDays(1))
            throw new ArgumentException("Tracking date cannot be in the future.", nameof(date));

        Value = date.Date;
    }

    public static implicit operator DateTime(TrackingDate date) => date.Value;
    public static implicit operator TrackingDate(DateTime date) => new(date);
}