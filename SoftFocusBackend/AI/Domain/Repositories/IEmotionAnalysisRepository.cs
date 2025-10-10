using SoftFocusBackend.AI.Domain.Model.Aggregates;

namespace SoftFocusBackend.AI.Domain.Repositories;

public interface IEmotionAnalysisRepository
{
    Task<EmotionAnalysis> SaveAsync(EmotionAnalysis analysis);
    Task<List<EmotionAnalysis>> GetUserAnalysesAsync(string userId, DateTime? from, DateTime? to, int pageSize);
    Task<List<EmotionAnalysis>> GetLast7DaysAsync(string userId);
    Task UpdateAsync(EmotionAnalysis analysis);
}
