using SoftFocusBackend.AI.Domain.Model.Aggregates;
using SoftFocusBackend.AI.Domain.Model.ValueObjects;

namespace SoftFocusBackend.AI.Domain.Services;

public interface ICrisisPatternDetector
{
    Task<CrisisPattern?> DetectFromChatAsync(ChatMessage message);
    Task<CrisisPattern?> DetectFromEmotionPatternAsync(string userId, EmotionAnalysis newAnalysis);
}
