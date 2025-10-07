using MongoDB.Bson.Serialization.Attributes;
using SoftFocusBackend.Shared.Domain.Entities;

namespace SoftFocusBackend.AI.Domain.Model.Aggregates;

public class ChatSession : BaseEntity
{
    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("startedAt")]
    public DateTime StartedAt { get; set; }

    [BsonElement("lastMessageAt")]
    public DateTime LastMessageAt { get; set; }

    [BsonElement("messageCount")]
    public int MessageCount { get; set; }

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    public void AddMessage()
    {
        MessageCount++;
        LastMessageAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Close()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public static ChatSession Create(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required", nameof(userId));

        var now = DateTime.UtcNow;
        return new ChatSession
        {
            UserId = userId,
            StartedAt = now,
            LastMessageAt = now,
            MessageCount = 0,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
