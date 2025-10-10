namespace SoftFocusBackend.Notification.Interfaces.REST.Resources;

public record NotificationPreferenceResource
{
    public string Id { get; init; } = string.Empty;
    public string NotificationType { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
    public string DeliveryMethod { get; init; } = string.Empty;
    public ScheduleResource? Schedule { get; init; }
    
    public record ScheduleResource
    {
        public List<QuietHourResource> QuietHours { get; init; } = new();
        public List<string> ActiveDays { get; init; } = new();
        public string TimeZone { get; init; } = "UTC";
    }
    
    public record QuietHourResource
    {
        public string StartTime { get; init; } = string.Empty;
        public string EndTime { get; init; } = string.Empty;
    }
}