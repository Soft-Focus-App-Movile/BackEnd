namespace SoftFocusBackend.Notification.Interfaces.REST.Resources;

public record SendNotificationRequest
{
    public string UserId { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string Priority { get; init; } = "Normal";
    public string? DeliveryMethod { get; init; }
    public DateTime? ScheduledAt { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
}