using MongoDB.Driver;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Context;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Repositories;

namespace SoftFocusBackend.Subscription.Infrastructure.Repositories;

public class SubscriptionRepository : BaseRepository<Domain.Aggregates.Subscription>, ISubscriptionRepository
{
    public SubscriptionRepository(MongoDbContext context) : base(context, "subscriptions")
    {
    }

    public async Task<Domain.Aggregates.Subscription?> GetByIdAsync(string id)
    {
        return await Collection.Find(s => s.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Domain.Aggregates.Subscription?> GetByUserIdAsync(string userId)
    {
        return await Collection.Find(s => s.UserId == userId).FirstOrDefaultAsync();
    }

    public async Task<Domain.Aggregates.Subscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId)
    {
        return await Collection.Find(s => s.StripeSubscriptionId == stripeSubscriptionId).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Domain.Aggregates.Subscription>> GetAllAsync()
    {
        return await Collection.Find(_ => true).ToListAsync();
    }

    public async Task<Domain.Aggregates.Subscription> CreateAsync(Domain.Aggregates.Subscription subscription)
    {
        subscription.CreatedAt = DateTime.UtcNow;
        subscription.UpdatedAt = DateTime.UtcNow;
        await Collection.InsertOneAsync(subscription);
        return subscription;
    }

    public async Task<Domain.Aggregates.Subscription> UpdateAsync(Domain.Aggregates.Subscription subscription)
    {
        subscription.UpdatedAt = DateTime.UtcNow;
        await Collection.ReplaceOneAsync(s => s.Id == subscription.Id, subscription);
        return subscription;
    }

    public async Task DeleteAsync(string id)
    {
        await Collection.DeleteOneAsync(s => s.Id == id);
    }

    public async Task<bool> ExistsAsync(string userId)
    {
        return await Collection.Find(s => s.UserId == userId).AnyAsync();
    }
}
