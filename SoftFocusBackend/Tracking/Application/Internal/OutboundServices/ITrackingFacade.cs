using SoftFocusBackend.Tracking.Domain.Model.Aggregates;

namespace SoftFocusBackend.Tracking.Application.Internal.OutboundServices;

public interface ITrackingFacade
{
    Task<CheckIn?> GetUserTodayCheckInAsync(string userId);
    Task<List<CheckIn>> GetUserCheckInsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<EmotionalCalendar?> GetUserEmotionalCalendarEntryByDateAsync(string userId, DateTime date);
    Task<List<EmotionalCalendar>> GetUserEmotionalCalendarAsync(string userId, DateTime? startDate = null, DateTime? endDate = null);
    Task<bool> HasUserCompletedCheckInTodayAsync(string userId);
}