using MongoDB.Driver;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Context;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Repositories;
using SoftFocusBackend.Tracking.Domain.Model.Aggregates;

namespace SoftFocusBackend.Tracking.Infrastructure.Persistence.MongoDB.Repositories;

public class EmotionalCalendarRepository : BaseRepository<EmotionalCalendar>, IEmotionalCalendarRepository
{
    public EmotionalCalendarRepository(MongoDbContext context) : base(context, "emotional_calendar")
    {
    }

    public async Task<EmotionalCalendar?> FindByUserIdAndDateAsync(string userId, DateTime date)
    {
        // Normalize the date to UTC and date only for comparison
        var normalizedDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        
        var filter = Builders<EmotionalCalendar>.Filter.And(
            Builders<EmotionalCalendar>.Filter.Eq(e => e.UserId, userId),
            Builders<EmotionalCalendar>.Filter.Gte(e => e.Date, normalizedDate),
            Builders<EmotionalCalendar>.Filter.Lt(e => e.Date, normalizedDate.AddDays(1))
        );

        return await Collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<List<EmotionalCalendar>> FindByUserIdAsync(string userId, int pageNumber = 1, int pageSize = 30)
    {
        var filter = Builders<EmotionalCalendar>.Filter.Eq(e => e.UserId, userId);
        var skip = (pageNumber - 1) * pageSize;

        return await Collection.Find(filter)
            .SortByDescending(e => e.Date)
            .Skip(skip)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<List<EmotionalCalendar>> FindByUserIdAndDateRangeAsync(string userId, DateTime startDate, DateTime endDate)
    {
        // Normalize dates to UTC and date only for comparison
        var normalizedStartDate = DateTime.SpecifyKind(startDate.Date, DateTimeKind.Utc);
        var normalizedEndDate = DateTime.SpecifyKind(endDate.Date.AddDays(1), DateTimeKind.Utc); // Add 1 day to include the end date
        
        var filter = Builders<EmotionalCalendar>.Filter.And(
            Builders<EmotionalCalendar>.Filter.Eq(e => e.UserId, userId),
            Builders<EmotionalCalendar>.Filter.Gte(e => e.Date, normalizedStartDate),
            Builders<EmotionalCalendar>.Filter.Lt(e => e.Date, normalizedEndDate)
        );

        return await Collection.Find(filter)
            .SortByDescending(e => e.Date)
            .ToListAsync();
    }
}