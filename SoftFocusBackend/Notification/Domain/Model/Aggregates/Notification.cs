using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SoftFocusBackend.Shared.Domain.Entities;
using SoftFocusBackend.Notification.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Notification.Domain.Model.Aggregates;

public class Notification : BaseEntity
{
    [BsonElement("user_id")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;
    
    [BsonElement("type")]
    public string Type { get; set; } = string.Empty;
    
    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;
    
    [BsonElement("content")]
    public string Content { get; set; } = string.Empty;
    
    [BsonElement("priority")]
    public string Priority { get; set; } = string.Empty;
    
    [BsonElement("status")]
    public string Status { get; set; } = string.Empty;
    
    [BsonElement("delivery_method")]
    public string DeliveryMethod { get; set; } = string.Empty;
    
    [BsonElement("scheduled_at")]
    public DateTime? ScheduledAt { get; set; }
    
    [BsonElement("sent_at")]
    public DateTime? SentAt { get; set; }
    
    [BsonElement("delivered_at")]
    public DateTime? DeliveredAt { get; set; }
    
    [BsonElement("read_at")]
    public DateTime? ReadAt { get; set; }
    
    [BsonElement("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    [BsonElement("retry_count")]
    public int RetryCount { get; set; }
    
    [BsonElement("last_error")]
    public string? LastError { get; set; }
    
    public void MarkAsSent()
    {
        Status = DeliveryStatus.Sent.ToString();
        SentAt = DateTime.UtcNow;
    }
    
    public void MarkAsDelivered()
    {
        Status = DeliveryStatus.Delivered.ToString();
        DeliveredAt = DateTime.UtcNow;
    }
    
    public void MarkAsFailed(string error)
    {
        Status = DeliveryStatus.Failed.ToString();
        LastError = error;
        RetryCount++;
    }
    
    public void MarkAsRead()
    {
        Status = DeliveryStatus.Read.ToString(); 
        ReadAt = DateTime.UtcNow;
    }
    
    public bool ShouldRetry()
    {
        return Status == DeliveryStatus.Failed.ToString() && RetryCount < 3;
    }
}