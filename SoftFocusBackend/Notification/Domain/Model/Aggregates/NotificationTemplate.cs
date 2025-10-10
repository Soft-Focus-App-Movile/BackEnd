using MongoDB.Bson.Serialization.Attributes;
using SoftFocusBackend.Shared.Domain.Entities;

namespace SoftFocusBackend.Notification.Domain.Model.Aggregates;

public class NotificationTemplate : BaseEntity
{
    [BsonElement("type")]
    public string Type { get; set; } = string.Empty;
    
    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;
    
    [BsonElement("body")]
    public string Body { get; set; } = string.Empty;
    
    [BsonElement("variables")]
    public List<string> Variables { get; set; } = new();
    
    [BsonElement("default_priority")]
    public string DefaultPriority { get; set; } = string.Empty;
    
    [BsonElement("supported_methods")]
    public List<string> SupportedMethods { get; set; } = new();
    
    public string RenderTitle(Dictionary<string, string> values)
    {
        var result = Title;
        foreach (var (key, value) in values)
        {
            result = result.Replace($"{{{{{key}}}}}", value);
        }
        return result;
    }
    
    public string RenderBody(Dictionary<string, string> values)
    {
        var result = Body;
        foreach (var (key, value) in values)
        {
            result = result.Replace($"{{{{{key}}}}}", value);
        }
        return result;
    }
}