using MongoDB.Driver;
using SoftFocusBackend.AI.Domain.Model.Aggregates;
using SoftFocusBackend.AI.Domain.Repositories;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Context;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Repositories;

namespace SoftFocusBackend.AI.Infrastructure.Persistence.MongoDB.Repositories;

public class MongoEmotionAnalysisRepository : BaseRepository<EmotionAnalysis>, IEmotionAnalysisRepository
{
    public MongoEmotionAnalysisRepository(MongoDbContext context) : base(context, "emotion_analyses")
    {
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        var userIndexKeys = Builders<EmotionAnalysis>.IndexKeys
            .Ascending(e => e.UserId)
            .Descending(e => e.AnalyzedAt);
        var userIndexModel = new CreateIndexModel<EmotionAnalysis>(userIndexKeys);
        Collection.Indexes.CreateOne(userIndexModel);

        var emotionIndexKeys = Builders<EmotionAnalysis>.IndexKeys.Ascending(e => e.DetectedEmotion);
        var emotionIndexModel = new CreateIndexModel<EmotionAnalysis>(emotionIndexKeys);
        Collection.Indexes.CreateOne(emotionIndexModel);
    }

    public async Task<EmotionAnalysis> SaveAsync(EmotionAnalysis analysis)
    {
        await Collection.InsertOneAsync(analysis);
        return analysis;
    }

    public async Task<List<EmotionAnalysis>> GetUserAnalysesAsync(string userId, DateTime? from, DateTime? to, int pageSize)
    {
        var filterBuilder = Builders<EmotionAnalysis>.Filter;
        var filter = filterBuilder.Eq(e => e.UserId, userId);

        if (from.HasValue)
        {
            filter = filterBuilder.And(filter, filterBuilder.Gte(e => e.AnalyzedAt, from.Value));
        }

        if (to.HasValue)
        {
            filter = filterBuilder.And(filter, filterBuilder.Lte(e => e.AnalyzedAt, to.Value));
        }

        return await Collection.Find(filter)
            .SortByDescending(e => e.AnalyzedAt)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<List<EmotionAnalysis>> GetLast7DaysAsync(string userId)
    {
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
        var filter = Builders<EmotionAnalysis>.Filter.And(
            Builders<EmotionAnalysis>.Filter.Eq(e => e.UserId, userId),
            Builders<EmotionAnalysis>.Filter.Gte(e => e.AnalyzedAt, sevenDaysAgo)
        );

        return await Collection.Find(filter)
            .SortBy(e => e.AnalyzedAt)
            .ToListAsync();
    }

    public async Task UpdateAsync(EmotionAnalysis analysis)
    {
        analysis.UpdatedAt = DateTime.UtcNow;
        await Collection.ReplaceOneAsync(e => e.Id == analysis.Id, analysis);
    }
}
