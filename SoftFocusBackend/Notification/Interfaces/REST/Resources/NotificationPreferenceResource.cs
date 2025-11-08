using System.Text.Json.Serialization;

namespace SoftFocusBackend.Notification.Interfaces.REST.Resources;

public record NotificationPreferenceResource
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("user_id")]
    public string UserId { get; init; } = string.Empty;

    [JsonPropertyName("notification_type")]
    public string NotificationType { get; init; } = string.Empty;

    [JsonPropertyName("is_enabled")]
    public bool IsEnabled { get; init; }

    [JsonPropertyName("delivery_method")]
    public string DeliveryMethod { get; init; } = string.Empty;

    [JsonPropertyName("schedule")]
    public ScheduleResource? Schedule { get; init; }
}

public record ScheduleResource
{
    [JsonPropertyName("start_time")]
    public string StartTime { get; init; } = string.Empty;

    [JsonPropertyName("end_time")]
    public string EndTime { get; init; } = string.Empty;

    [JsonPropertyName("days_of_week")]
    public List<int> DaysOfWeek { get; init; } = new();
}