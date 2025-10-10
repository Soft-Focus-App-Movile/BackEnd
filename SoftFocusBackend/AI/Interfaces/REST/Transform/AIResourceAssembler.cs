using SoftFocusBackend.AI.Domain.Model.Aggregates;
using SoftFocusBackend.AI.Domain.Model.Commands;
using SoftFocusBackend.AI.Domain.Services;
using SoftFocusBackend.AI.Interfaces.REST.Resources;

namespace SoftFocusBackend.AI.Interfaces.REST.Transform;

public static class AIResourceAssembler
{
    public static SendChatMessageCommand ToCommand(ChatMessageRequest request, string userId, string? ipAddress = null)
    {
        return new SendChatMessageCommand(userId, request.Message, request.SessionId, ipAddress);
    }

    public static AnalyzeFacialEmotionCommand ToCommand(byte[] imageBytes, string userId, bool autoCheckIn)
    {
        return new AnalyzeFacialEmotionCommand(userId, imageBytes, autoCheckIn);
    }

    public static ChatMessageResponse ToChatResponse(string sessionId, ChatResponse response)
    {
        return new ChatMessageResponse
        {
            SessionId = sessionId,
            Reply = response.Reply,
            SuggestedQuestions = response.SuggestedQuestions,
            RecommendedExercises = response.RecommendedExercises
                .Select(e => new ExerciseRecommendation { Id = e, Title = e, Duration = "5 min" })
                .ToList(),
            CrisisDetected = response.CrisisDetected,
            Timestamp = DateTime.UtcNow
        };
    }

    public static EmotionAnalysisResponse ToEmotionResponse(EmotionAnalysis analysis)
    {
        return new EmotionAnalysisResponse
        {
            AnalysisId = analysis.Id,
            Emotion = analysis.DetectedEmotion,
            Confidence = analysis.Confidence,
            AllEmotions = analysis.AllEmotions,
            AnalyzedAt = analysis.AnalyzedAt,
            CheckInCreated = analysis.CheckInCreated,
            CheckInId = analysis.CheckInId
        };
    }

    public static AIUsageStatsResponse ToUsageStatsResponse(AIUsage usage)
    {
        return new AIUsageStatsResponse
        {
            ChatMessagesUsed = usage.ChatMessagesUsed,
            ChatMessagesLimit = usage.ChatMessagesLimit,
            FacialAnalysisUsed = usage.FacialAnalysisUsed,
            FacialAnalysisLimit = usage.FacialAnalysisLimit,
            RemainingMessages = usage.RemainingChatMessages(),
            RemainingAnalyses = usage.RemainingFacialAnalyses(),
            CurrentWeek = usage.Week,
            ResetsAt = usage.GetResetDate(),
            Plan = usage.Plan
        };
    }

    public static object ToErrorResponse(string message, AIUsage? usage = null)
    {
        var error = new
        {
            error = true,
            message
        };

        if (usage != null)
        {
            return new
            {
                error = true,
                message,
                usageStats = ToUsageStatsResponse(usage)
            };
        }

        return error;
    }
}
