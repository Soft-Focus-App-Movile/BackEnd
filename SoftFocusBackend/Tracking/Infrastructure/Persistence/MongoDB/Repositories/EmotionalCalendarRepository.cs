using MongoDB.Driver;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Context;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Repositories;
using SoftFocusBackend.Tracking.Domain.Model.Aggregates;

namespace SoftFocusBackend.Tracking.Infrastructure.Persistence.MongoDB.Repositories;

public class EmotionalCalendarRepository : BaseRepository<EmotionalCalendar>, IEmotionalCalendarRepository
{
    public EmotionalCalendarRepository(MongoDbContext context) : base(context, "emotional_calendar")
    {
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        try
        {
            var indexKeys = Builders<EmotionalCalendar>.IndexKeys
                .Ascending(e => e.UserId)
                .Descending(e => e.Timestamp);

            var indexModel = new CreateIndexModel<EmotionalCalendar>(
                indexKeys,
                new CreateIndexOptions { Name = "idx_userId_timestamp", Background = true });

            Collection.Indexes.CreateOne(indexModel);
        }
        catch
        {
            // Index creation is best-effort; ignore failures (e.g. offline DB at startup)
        }
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

    public async Task<List<EmotionalCalendar>> GetUserEntriesByDateAsync(string userId, DateTime date)
    {
        // Returns ALL entries for the given date (multiple entries per day are allowed)
        var startOfDay = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        var endOfDay = startOfDay.AddDays(1);

        var filter = Builders<EmotionalCalendar>.Filter.And(
            Builders<EmotionalCalendar>.Filter.Eq(e => e.UserId, userId),
            Builders<EmotionalCalendar>.Filter.Or(
                Builders<EmotionalCalendar>.Filter.And(
                    Builders<EmotionalCalendar>.Filter.Gte(e => e.Timestamp, startOfDay),
                    Builders<EmotionalCalendar>.Filter.Lt(e => e.Timestamp, endOfDay)),
                // Legacy entries created before the 24h diary experiment have no Timestamp
                Builders<EmotionalCalendar>.Filter.And(
                    Builders<EmotionalCalendar>.Filter.Eq(e => e.Timestamp, default(DateTime)),
                    Builders<EmotionalCalendar>.Filter.Gte(e => e.Date, startOfDay),
                    Builders<EmotionalCalendar>.Filter.Lt(e => e.Date, endOfDay))
            )
        );

        return await Collection.Find(filter)
            .SortBy(e => e.Timestamp)
            .ToListAsync();
    }

    public async Task<List<EmotionalCalendar>> GetUserEntriesByHourAsync(string userId, DateTime dateTime)
    {
        // Returns all entries within the specific hour of the given DateTime
        var utc = dateTime.Kind == DateTimeKind.Utc ? dateTime : dateTime.ToUniversalTime();
        var startOfHour = new DateTime(utc.Year, utc.Month, utc.Day, utc.Hour, 0, 0, DateTimeKind.Utc);
        var endOfHour = startOfHour.AddHours(1);

        var filter = Builders<EmotionalCalendar>.Filter.And(
            Builders<EmotionalCalendar>.Filter.Eq(e => e.UserId, userId),
            Builders<EmotionalCalendar>.Filter.Gte(e => e.Timestamp, startOfHour),
            Builders<EmotionalCalendar>.Filter.Lt(e => e.Timestamp, endOfHour)
        );

        return await Collection.Find(filter)
            .SortBy(e => e.Timestamp)
            .ToListAsync();
    }
}