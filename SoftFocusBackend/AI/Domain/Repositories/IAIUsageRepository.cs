using SoftFocusBackend.AI.Domain.Model.Aggregates;

namespace SoftFocusBackend.AI.Domain.Repositories;

public interface IAIUsageRepository
{
    Task<AIUsage?> GetCurrentWeekUsageAsync(string userId);
    Task<AIUsage> CreateOrUpdateAsync(AIUsage usage);
    Task IncrementUsageAsync(string userId, string featureType);
}
