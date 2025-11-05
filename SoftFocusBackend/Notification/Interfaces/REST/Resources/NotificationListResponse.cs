using System.Text.Json.Serialization;

namespace SoftFocusBackend.Notification.Interfaces.REST.Resources;

public class NotificationListResponse
{
    [JsonPropertyName("notifications")]
    public List<NotificationResource> Notifications { get; set; } = new();

    [JsonPropertyName("totalCount")] 
    public int TotalCount { get; set; }

    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }
}