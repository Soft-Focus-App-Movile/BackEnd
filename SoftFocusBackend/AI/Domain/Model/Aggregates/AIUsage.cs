using MongoDB.Bson.Serialization.Attributes;
using SoftFocusBackend.Shared.Domain.Entities;

namespace SoftFocusBackend.AI.Domain.Model.Aggregates;

public class AIUsage : BaseEntity
{
    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("week")]
    public string Week { get; set; } = string.Empty;

    [BsonElement("weekStartDate")]
    public DateTime WeekStartDate { get; set; }

    [BsonElement("chatMessagesUsed")]
    public int ChatMessagesUsed { get; set; }

    [BsonElement("chatMessagesLimit")]
    public int ChatMessagesLimit { get; set; }

    [BsonElement("facialAnalysisUsed")]
    public int FacialAnalysisUsed { get; set; }

    [BsonElement("facialAnalysisLimit")]
    public int FacialAnalysisLimit { get; set; }

    [BsonElement("plan")]
    public string Plan { get; set; } = "Free";

    public void IncrementChatUsage()
    {
        ChatMessagesUsed++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementFacialAnalysisUsage()
    {
        FacialAnalysisUsed++;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool CanUseChat() => ChatMessagesUsed < ChatMessagesLimit;
    public bool CanUseFacialAnalysis() => FacialAnalysisUsed < FacialAnalysisLimit;

    public int RemainingChatMessages() => Math.Max(0, ChatMessagesLimit - ChatMessagesUsed);
    public int RemainingFacialAnalyses() => Math.Max(0, FacialAnalysisLimit - FacialAnalysisUsed);

    public DateTime GetResetDate() => WeekStartDate.AddDays(7);

    public static AIUsage Create(string userId, string week, DateTime weekStartDate, string plan,
        int chatLimit, int facialLimit)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required", nameof(userId));

        if (string.IsNullOrWhiteSpace(week))
            throw new ArgumentException("Week is required", nameof(week));

        if (string.IsNullOrWhiteSpace(plan))
            throw new ArgumentException("Plan is required", nameof(plan));

        var now = DateTime.UtcNow;
        return new AIUsage
        {
            UserId = userId,
            Week = week,
            WeekStartDate = weekStartDate,
            ChatMessagesUsed = 0,
            ChatMessagesLimit = chatLimit,
            FacialAnalysisUsed = 0,
            FacialAnalysisLimit = facialLimit,
            Plan = plan,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}