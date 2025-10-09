namespace SoftFocusBackend.Notification.Domain.Model.Commands;

public record SendNotificationCommand(
    string UserId,
    string Type,
    string Title,
    string Content,
    string Priority,
    string? DeliveryMethod = null,
    DateTime? ScheduledAt = null,
    Dictionary<string, object>? Metadata = null
);