namespace SoftFocusBackend.Crisis.Domain.Model.ValueObjects;

public record Location
{
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }

    public Location(double? latitude, double? longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public bool HasCoordinates() => Latitude.HasValue && Longitude.HasValue;

    public string ToDisplayString()
    {
        if (!HasCoordinates()) return "No location available";
        return $"{Latitude:F6}, {Longitude:F6}";
    }
}
