namespace SoftFocusBackend.Notification.Domain.Model.ValueObjects;

public record DeliveryStatus
{
    public string Value { get; }
    
    private DeliveryStatus(string value) => Value = value;
    
    public static readonly DeliveryStatus Pending = new("Pending");
    public static readonly DeliveryStatus Sent = new("Sent");
    public static readonly DeliveryStatus Delivered = new("Delivered");
    public static readonly DeliveryStatus Failed = new("Failed");
    public static readonly DeliveryStatus Cancelled = new("Cancelled");
    
    public static DeliveryStatus FromString(string value)
    {
        return value switch
        {
            "Pending" => Pending,
            "Sent" => Sent,
            "Delivered" => Delivered,
            "Failed" => Failed,
            "Cancelled" => Cancelled,
            _ => throw new ArgumentException($"Invalid delivery status: {value}")
        };
    }
    
    public override string ToString() => Value;
}
