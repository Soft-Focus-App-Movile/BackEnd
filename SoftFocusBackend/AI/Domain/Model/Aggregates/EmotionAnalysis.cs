using MongoDB.Bson.Serialization.Attributes;
using SoftFocusBackend.Shared.Domain.Entities;

namespace SoftFocusBackend.AI.Domain.Model.Aggregates;

public class EmotionAnalysis : BaseEntity
{
    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("detectedEmotion")]
    public string DetectedEmotion { get; set; } = string.Empty;

    [BsonElement("confidence")]
    public double Confidence { get; set; }

    [BsonElement("allEmotions")]
    public Dictionary<string, double> AllEmotions { get; set; } = new();

    [BsonElement("imageUrl")]
    public string ImageUrl { get; set; } = string.Empty;

    [BsonElement("checkInCreated")]
    public bool CheckInCreated { get; set; }

    [BsonElement("checkInId")]
    public string? CheckInId { get; set; }

    [BsonElement("analyzedAt")]
    public DateTime AnalyzedAt { get; set; }

    public static EmotionAnalysis Create(string userId, string detectedEmotion, double confidence,
        Dictionary<string, double> allEmotions, string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required", nameof(userId));

        if (string.IsNullOrWhiteSpace(detectedEmotion))
            throw new ArgumentException("DetectedEmotion is required", nameof(detectedEmotion));

        if (confidence < 0 || confidence > 1)
            throw new ArgumentException("Confidence must be between 0 and 1", nameof(confidence));

        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("ImageUrl is required", nameof(imageUrl));

        var now = DateTime.UtcNow;
        return new EmotionAnalysis
        {
            UserId = userId,
            DetectedEmotion = detectedEmotion,
            Confidence = confidence,
            AllEmotions = allEmotions,
            ImageUrl = imageUrl,
            AnalyzedAt = now,
            CheckInCreated = false,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void MarkCheckInCreated(string checkInId)
    {
        CheckInCreated = true;
        CheckInId = checkInId;
        UpdatedAt = DateTime.UtcNow;
    }
}