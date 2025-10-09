namespace SoftFocusBackend.Notification.Domain.Model.ValueObjects;

public record NotificationSchedule
{
    public List<TimeSpan> QuietHours { get; init; }
    public DayOfWeek[] ActiveDays { get; init; }
    public TimeZoneInfo TimeZone { get; init; }
    
    public NotificationSchedule()
    {
        QuietHours = new List<TimeSpan>();
        ActiveDays = Enum.GetValues<DayOfWeek>();
        TimeZone = TimeZoneInfo.Local;
    }
    
    public bool IsDeliveryAllowed(DateTime dateTime)
    {
        var localTime = TimeZoneInfo.ConvertTime(dateTime, TimeZone);
        
        if (!ActiveDays.Contains(localTime.DayOfWeek))
            return false;
        
        var currentTime = localTime.TimeOfDay;
        foreach (var quietHour in QuietHours)
        {
            if (currentTime >= quietHour && currentTime < quietHour.Add(TimeSpan.FromHours(1)))
                return false;
        }
        
        return true;
    }
}
