namespace SoftFocusBackend.Notification.Domain.Model.ValueObjects;

public record Priority
{
    public string Level { get; }
    public int Value { get; }
    public TimeSpan MaxDeliveryTime { get; }
    
    private Priority(string level, int value, TimeSpan maxDeliveryTime)
    {
        Level = level;
        Value = value;
        MaxDeliveryTime = maxDeliveryTime;
    }
    
    public static readonly Priority Low = new("Low", 0, TimeSpan.FromHours(24));
    public static readonly Priority Normal = new("Normal", 1, TimeSpan.FromHours(4));
    public static readonly Priority High = new("High", 2, TimeSpan.FromHours(1));
    public static readonly Priority Critical = new("Critical", 3, TimeSpan.FromMinutes(5));
    
    public static Priority FromString(string level)
    {
        return level switch
        {
            "Low" => Low,
            "Normal" => Normal,
            "High" => High,
            "Critical" => Critical,
            _ => throw new ArgumentException($"Invalid priority level: {level}")
        };
    }
    
    public static Priority FromValue(int value)
    {
        return value switch
        {
            0 => Low,
            1 => Normal,
            2 => High,
            3 => Critical,
            _ => throw new ArgumentException($"Invalid priority value: {value}")
        };
    }
    
    public override string ToString() => Level;
}
