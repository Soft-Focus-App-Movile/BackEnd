namespace SoftFocusBackend.Notification.Domain.Model.Commands;

public record UpdatePreferencesCommand(
    string UserId,
    string NotificationType,
    bool IsEnabled,
    string? DeliveryMethod = null,
    object? Schedule = null,
    bool? PreviousIsEnabled = null 
);