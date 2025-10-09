namespace SoftFocusBackend.Notification.Domain.Model.ValueObjects;

public record NotificationType
{
    public string Value { get; }
    
    private NotificationType(string value) => Value = value;
    
    public static readonly NotificationType CheckinReminder = new("CheckinReminder");
    public static readonly NotificationType CrisisAlert = new("CrisisAlert");
    public static readonly NotificationType MessageReceived = new("MessageReceived");
    public static readonly NotificationType AssignmentDue = new("AssignmentDue");
    public static readonly NotificationType TherapyReminder = new("TherapyReminder");
    public static readonly NotificationType EmotionalInsight = new("EmotionalInsight");
    
    public static NotificationType FromString(string value)
    {
        return value switch
        {
            "CheckinReminder" => CheckinReminder,
            "CrisisAlert" => CrisisAlert,
            "MessageReceived" => MessageReceived,
            "AssignmentDue" => AssignmentDue,
            "TherapyReminder" => TherapyReminder,
            "EmotionalInsight" => EmotionalInsight,
            _ => throw new ArgumentException($"Invalid notification type: {value}")
        };
    }
    
    public override string ToString() => Value;
}