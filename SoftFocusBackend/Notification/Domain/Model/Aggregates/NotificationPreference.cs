using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SoftFocusBackend.Shared.Domain.Entities;

namespace SoftFocusBackend.Notification.Domain.Model.Aggregates;

public class NotificationPreference : BaseEntity
{
    [BsonElement("user_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;
    
    [BsonElement("notification_type")]
    public string NotificationType { get; set; } = string.Empty;
    
    [BsonElement("is_enabled")]
    public bool IsEnabled { get; set; } = true;
    
    [BsonElement("delivery_method")]
    public string DeliveryMethod { get; set; } = string.Empty;
    
    [BsonElement("schedule")]
    public ScheduleSettings? Schedule { get; set; }
    
    // ✅ NUEVO CAMPO: Timestamp de cuando se desactivaron las notificaciones
    [BsonElement("disabled_at")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? DisabledAt { get; set; }
    
    public class ScheduleSettings
    {
        [BsonElement("quiet_hours")]
        public List<QuietHourRange> QuietHours { get; set; } = new();
        
        [BsonElement("active_days")]
        public List<string> ActiveDays { get; set; } = new();
        
        [BsonElement("timezone")]
        public string TimeZone { get; set; } = "UTC";
        
        public class QuietHourRange
        {
            [BsonElement("start_time")]
            public string StartTime { get; set; } = string.Empty;
            
            [BsonElement("end_time")]
            public string EndTime { get; set; } = string.Empty;
        }
    }
}