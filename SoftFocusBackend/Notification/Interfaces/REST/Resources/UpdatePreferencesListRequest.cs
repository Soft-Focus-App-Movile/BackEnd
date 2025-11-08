using System.Text.Json.Serialization;

namespace SoftFocusBackend.Notification.Interfaces.REST.Resources;

/// <summary>
/// Request para actualizar MÚLTIPLES preferencias a la vez
/// </summary>
public class UpdatePreferencesListRequest
{
    [JsonPropertyName("preferences")]
    public List<PreferenceUpdateItem> Preferences { get; set; } = new();

    public class PreferenceUpdateItem
    {
        [JsonPropertyName("notification_type")]
        public string NotificationType { get; set; } = string.Empty;

        [JsonPropertyName("is_enabled")]
        public bool IsEnabled { get; set; }

        [JsonPropertyName("delivery_method")]
        public string DeliveryMethod { get; set; } = string.Empty;

        [JsonPropertyName("schedule")]
        public ScheduleUpdateItem? Schedule { get; set; }
    }

    public class ScheduleUpdateItem
    {
        [JsonPropertyName("start_time")]
        public string StartTime { get; set; } = string.Empty;

        [JsonPropertyName("end_time")]
        public string EndTime { get; set; } = string.Empty;

        [JsonPropertyName("days_of_week")]
        public List<int> DaysOfWeek { get; set; } = new();
    }
}