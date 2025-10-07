using SoftFocusBackend.AI.Domain.Model.Aggregates;

namespace SoftFocusBackend.AI.Domain.Services;

public interface IAIUsageTracker
{
    Task<bool> CanUseChatAsync(string userId);
    Task<bool> CanUseFacialAnalysisAsync(string userId);
    Task IncrementChatUsageAsync(string userId);
    Task IncrementFacialUsageAsync(string userId);
    Task<AIUsage> GetCurrentUsageAsync(string userId);
}
