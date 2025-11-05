using System.Text.Json.Serialization;

namespace SoftFocusBackend.Notification.Interfaces.REST.Resources;

public class PreferenceListResponse
{
    [JsonPropertyName("preferences")]
    public List<NotificationPreferenceResource> Preferences { get; set; } = new();

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}