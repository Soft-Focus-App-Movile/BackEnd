using SoftFocusBackend.Shared.Domain.Repositories;
using SoftFocusBackend.Tracking.Domain.Model.Aggregates;

namespace SoftFocusBackend.Tracking.Infrastructure.Persistence.MongoDB.Repositories;

public interface ICheckInRepository : IBaseRepository<CheckIn>
{
    Task<CheckIn?> FindTodayCheckInByUserIdAsync(string userId);
    Task<List<CheckIn>> FindByUserIdAsync(string userId, DateTime? startDate = null, DateTime? endDate = null, int pageNumber = 1, int pageSize = 20);
}