using MongoDB.Driver;
using SoftFocusBackend.AI.Domain.Model.Aggregates;
using SoftFocusBackend.AI.Domain.Repositories;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Context;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Repositories;
using System.Globalization;

namespace SoftFocusBackend.AI.Infrastructure.Persistence.MongoDB.Repositories;

public class MongoAIUsageRepository : BaseRepository<AIUsage>, IAIUsageRepository
{
    public MongoAIUsageRepository(MongoDbContext context) : base(context, "ai_usage")
    {
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        var userWeekIndexKeys = Builders<AIUsage>.IndexKeys
            .Ascending(u => u.UserId)
            .Ascending(u => u.Week);
        var userWeekIndexOptions = new CreateIndexOptions { Unique = true };
        var userWeekIndexModel = new CreateIndexModel<AIUsage>(userWeekIndexKeys, userWeekIndexOptions);
        Collection.Indexes.CreateOne(userWeekIndexModel);

        var weekStartIndexKeys = Builders<AIUsage>.IndexKeys.Ascending(u => u.WeekStartDate);
        var weekStartIndexModel = new CreateIndexModel<AIUsage>(weekStartIndexKeys);
        Collection.Indexes.CreateOne(weekStartIndexModel);
    }

    public async Task<AIUsage?> GetCurrentWeekUsageAsync(string userId)
    {
        var (week, _) = GetCurrentWeek();
        return await Collection.Find(u => u.UserId == userId && u.Week == week).FirstOrDefaultAsync();
    }

    public async Task<AIUsage> CreateOrUpdateAsync(AIUsage usage)
    {
        var existing = await Collection.Find(u => u.UserId == usage.UserId && u.Week == usage.Week).FirstOrDefaultAsync();

        if (existing != null)
        {
            existing.ChatMessagesUsed = usage.ChatMessagesUsed;
            existing.ChatMessagesLimit = usage.ChatMessagesLimit;
            existing.FacialAnalysisUsed = usage.FacialAnalysisUsed;
            existing.FacialAnalysisLimit = usage.FacialAnalysisLimit;
            existing.Plan = usage.Plan;
            existing.UpdatedAt = DateTime.UtcNow;
            await Collection.ReplaceOneAsync(u => u.Id == existing.Id, existing);
            return existing;
        }

        await Collection.InsertOneAsync(usage);
        return usage;
    }

    public async Task IncrementUsageAsync(string userId, string featureType)
    {
        var (week, weekStart) = GetCurrentWeek();

        var filter = Builders<AIUsage>.Filter.And(
            Builders<AIUsage>.Filter.Eq(u => u.UserId, userId),
            Builders<AIUsage>.Filter.Eq(u => u.Week, week)
        );

        var update = featureType.ToLower() == "chat"
            ? Builders<AIUsage>.Update
                .Inc(u => u.ChatMessagesUsed, 1)
                .Set(u => u.UpdatedAt, DateTime.UtcNow)
            : Builders<AIUsage>.Update
                .Inc(u => u.FacialAnalysisUsed, 1)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

        var options = new FindOneAndUpdateOptions<AIUsage>
        {
            IsUpsert = false,
            ReturnDocument = ReturnDocument.After
        };

        await Collection.FindOneAndUpdateAsync(filter, update, options);
    }

    private (string week, DateTime weekStart) GetCurrentWeek()
    {
        var now = DateTime.UtcNow;
        var daysSinceMonday = ((int)now.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        var weekStart = now.Date.AddDays(-daysSinceMonday);

        var calendar = CultureInfo.InvariantCulture.Calendar;
        var weekNumber = calendar.GetWeekOfYear(weekStart, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        var week = $"{weekStart.Year}-W{weekNumber:D2}";

        return (week, weekStart);
    }
}
