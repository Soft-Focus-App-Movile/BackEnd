namespace SoftFocusBackend.Notification.Interfaces.REST.Resources;

public record UpdatePreferenceRequest
{
    public string NotificationType { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
    public string? DeliveryMethod { get; init; }
    public object? Schedule { get; init; }
}