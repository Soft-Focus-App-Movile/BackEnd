using MongoDB.Driver;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Context;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Repositories;
using SoftFocusBackend.Tracking.Domain.Model.Aggregates;

namespace SoftFocusBackend.Tracking.Infrastructure.Persistence.MongoDB.Repositories;

public class CheckInRepository : BaseRepository<CheckIn>, ICheckInRepository
{
    public CheckInRepository(MongoDbContext context) : base(context, "checkins")
    {
    }

    public async Task<CheckIn?> FindTodayCheckInByUserIdAsync(string userId)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);
        
        var filter = Builders<CheckIn>.Filter.And(
            Builders<CheckIn>.Filter.Eq(c => c.UserId, userId),
            Builders<CheckIn>.Filter.Gte(c => c.CompletedAt, today),
            Builders<CheckIn>.Filter.Lt(c => c.CompletedAt, tomorrow)
        );

        return await Collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<List<CheckIn>> FindByUserIdAsync(string userId, DateTime? startDate = null, DateTime? endDate = null, int pageNumber = 1, int pageSize = 20)
    {
        var filterBuilder = Builders<CheckIn>.Filter;
        var filter = filterBuilder.Eq(c => c.UserId, userId);

        if (startDate.HasValue)
        {
            filter = filterBuilder.And(filter, filterBuilder.Gte(c => c.CompletedAt, startDate.Value));
        }

        if (endDate.HasValue)
        {
            filter = filterBuilder.And(filter, filterBuilder.Lte(c => c.CompletedAt, endDate.Value));
        }

        var skip = (pageNumber - 1) * pageSize;

        return await Collection.Find(filter)
            .SortByDescending(c => c.CompletedAt)
            .Skip(skip)
            .Limit(pageSize)
            .ToListAsync();
    }
}