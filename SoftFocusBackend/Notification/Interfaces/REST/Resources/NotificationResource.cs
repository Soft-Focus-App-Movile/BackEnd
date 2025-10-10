namespace SoftFocusBackend.Notification.Interfaces.REST.Resources;

public record NotificationResource
{
    public string Id { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string Priority { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string DeliveryMethod { get; init; } = string.Empty;
    public DateTime? ScheduledAt { get; init; }
    public DateTime? DeliveredAt { get; init; }
    public DateTime? ReadAt { get; init; }
    public DateTime CreatedAt { get; init; }
}