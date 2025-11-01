using SoftFocusBackend.Subscription.Domain.Aggregates;
using SoftFocusBackend.Subscription.Domain.ValueObjects;

namespace SoftFocusBackend.Subscription.Infrastructure.Repositories;

public interface IUsageTrackingRepository
{
    Task<UsageTracking?> GetByIdAsync(string id);
    Task<UsageTracking?> GetByUserAndFeatureAsync(string userId, FeatureType featureType);
    Task<IEnumerable<UsageTracking>> GetAllByUserIdAsync(string userId);
    Task<UsageTracking> CreateAsync(UsageTracking usageTracking);
    Task<UsageTracking> UpdateAsync(UsageTracking usageTracking);
    Task DeleteAsync(string id);
    Task DeleteAllByUserIdAsync(string userId);
}
