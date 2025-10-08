using SoftFocusBackend.Shared.Domain.Repositories;
using SoftFocusBackend.Tracking.Domain.Model.Aggregates;

namespace SoftFocusBackend.Tracking.Infrastructure.Persistence.MongoDB.Repositories;

public interface IEmotionalCalendarRepository : IBaseRepository<EmotionalCalendar>
{
    Task<EmotionalCalendar?> FindByUserIdAndDateAsync(string userId, DateTime date);
    Task<List<EmotionalCalendar>> FindByUserIdAsync(string userId, int pageNumber = 1, int pageSize = 30);
    Task<List<EmotionalCalendar>> FindByUserIdAndDateRangeAsync(string userId, DateTime startDate, DateTime endDate);
}