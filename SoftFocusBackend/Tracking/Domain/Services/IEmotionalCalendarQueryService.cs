using SoftFocusBackend.Tracking.Domain.Model.Aggregates;
using SoftFocusBackend.Tracking.Domain.Model.Queries;

namespace SoftFocusBackend.Tracking.Domain.Services;

public interface IEmotionalCalendarQueryService
{
    Task<EmotionalCalendar?> HandleGetEmotionalCalendarEntryByDateAsync(GetEmotionalCalendarEntryByDateQuery query);
    Task<List<EmotionalCalendar>> HandleGetUserEmotionalCalendarAsync(GetUserEmotionalCalendarQuery query);
    Task<List<EmotionalCalendar>> HandleGetEmotionalCalendarByDateRangeAsync(GetEmotionalCalendarByDateRangeQuery query);
}