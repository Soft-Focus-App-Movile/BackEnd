namespace SoftFocusBackend.Crisis.Interfaces.REST.Resources;

public record CreateCrisisAlertResource(
    double? Latitude,
    double? Longitude
);
