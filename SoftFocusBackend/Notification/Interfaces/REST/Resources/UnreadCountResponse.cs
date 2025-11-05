using System.Text.Json.Serialization;

namespace SoftFocusBackend.Notification.Interfaces.REST.Resources;

public class UnreadCountResponse
{
    [JsonPropertyName("unreadCount")]
    public int UnreadCount { get; set; }
}