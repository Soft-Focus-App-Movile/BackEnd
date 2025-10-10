namespace SoftFocusBackend.Notification.Domain.Model.ValueObjects;

public record DeliveryMethod
{
    public string Value { get; }
    
    private DeliveryMethod(string value) => Value = value;
    
    public static readonly DeliveryMethod Push = new("Push");
    public static readonly DeliveryMethod Email = new("Email");
    public static readonly DeliveryMethod InApp = new("InApp");
    public static readonly DeliveryMethod SMS = new("SMS");
    
    public static DeliveryMethod FromString(string value)
    {
        return value switch
        {
            "Push" => Push,
            "Email" => Email,
            "InApp" => InApp,
            "SMS" => SMS,
            _ => throw new ArgumentException($"Invalid delivery method: {value}")
        };
    }
    
    public override string ToString() => Value;
}