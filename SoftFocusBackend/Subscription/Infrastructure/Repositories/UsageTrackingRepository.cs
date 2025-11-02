using MongoDB.Driver;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Context;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Repositories;
using SoftFocusBackend.Subscription.Domain.Aggregates;
using SoftFocusBackend.Subscription.Domain.ValueObjects;

namespace SoftFocusBackend.Subscription.Infrastructure.Repositories;

public class UsageTrackingRepository : BaseRepository<UsageTracking>, IUsageTrackingRepository
{
    public UsageTrackingRepository(MongoDbContext context) : base(context, "usageTrackings")
    {
    }

    public async Task<UsageTracking?> GetByIdAsync(string id)
    {
        return await Collection.Find(u => u.Id == id).FirstOrDefaultAsync();
    }

    public async Task<UsageTracking?> GetByUserAndFeatureAsync(string userId, FeatureType featureType)
    {
        return await Collection
            .Find(u => u.UserId == userId && u.FeatureType == featureType)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<UsageTracking>> GetAllByUserIdAsync(string userId)
    {
        return await Collection.Find(u => u.UserId == userId).ToListAsync();
    }

    public async Task<UsageTracking> CreateAsync(UsageTracking usageTracking)
    {
        usageTracking.CreatedAt = DateTime.UtcNow;
        usageTracking.UpdatedAt = DateTime.UtcNow;
        await Collection.InsertOneAsync(usageTracking);
        return usageTracking;
    }

    public async Task<UsageTracking> UpdateAsync(UsageTracking usageTracking)
    {
        usageTracking.UpdatedAt = DateTime.UtcNow;
        await Collection.ReplaceOneAsync(u => u.Id == usageTracking.Id, usageTracking);
        return usageTracking;
    }

    public async Task DeleteAsync(string id)
    {
        await Collection.DeleteOneAsync(u => u.Id == id);
    }

    public async Task DeleteAllByUserIdAsync(string userId)
    {
        await Collection.DeleteManyAsync(u => u.UserId == userId);
    }
}
