using SoftFocusBackend.Subscription.Domain.Aggregates;

namespace SoftFocusBackend.Subscription.Infrastructure.Repositories;

public interface ISubscriptionRepository
{
    Task<Domain.Aggregates.Subscription?> GetByIdAsync(string id);
    Task<Domain.Aggregates.Subscription?> GetByUserIdAsync(string userId);
    Task<Domain.Aggregates.Subscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId);
    Task<IEnumerable<Domain.Aggregates.Subscription>> GetAllAsync();
    Task<Domain.Aggregates.Subscription> CreateAsync(Domain.Aggregates.Subscription subscription);
    Task<Domain.Aggregates.Subscription> UpdateAsync(Domain.Aggregates.Subscription subscription);
    Task DeleteAsync(string id);
    Task<bool> ExistsAsync(string userId);
}
