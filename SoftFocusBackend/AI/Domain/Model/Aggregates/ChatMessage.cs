using MongoDB.Bson.Serialization.Attributes;
using SoftFocusBackend.Shared.Domain.Entities;
using SoftFocusBackend.AI.Domain.Model.ValueObjects;

namespace SoftFocusBackend.AI.Domain.Model.Aggregates;

public class ChatMessage : BaseEntity
{
    [BsonElement("sessionId")]
    public string SessionId { get; set; } = string.Empty;

    [BsonElement("role")]
    public ChatRole Role { get; set; }

    [BsonElement("content")]
    public string Content { get; set; } = string.Empty;

    [BsonElement("suggestedQuestions")]
    public List<string> SuggestedQuestions { get; set; } = new();

    [BsonElement("recommendedExercises")]
    public List<string> RecommendedExercises { get; set; } = new();

    [BsonElement("crisisDetected")]
    public bool CrisisDetected { get; set; }

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; }

    public static ChatMessage CreateUserMessage(string sessionId, string content)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("SessionId is required", nameof(sessionId));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content is required", nameof(content));

        var now = DateTime.UtcNow;
        return new ChatMessage
        {
            SessionId = sessionId,
            Role = ChatRole.User,
            Content = content,
            Timestamp = now,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public static ChatMessage CreateAssistantMessage(string sessionId, string content,
        List<string>? suggestedQuestions = null, List<string>? recommendedExercises = null,
        bool crisisDetected = false)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("SessionId is required", nameof(sessionId));

        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content is required", nameof(content));

        var now = DateTime.UtcNow;
        return new ChatMessage
        {
            SessionId = sessionId,
            Role = ChatRole.Assistant,
            Content = content,
            SuggestedQuestions = suggestedQuestions ?? new List<string>(),
            RecommendedExercises = recommendedExercises ?? new List<string>(),
            CrisisDetected = crisisDetected,
            Timestamp = now,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}