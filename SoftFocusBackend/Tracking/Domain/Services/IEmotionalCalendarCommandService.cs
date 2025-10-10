using SoftFocusBackend.Tracking.Domain.Model.Aggregates;
using SoftFocusBackend.Tracking.Domain.Model.Commands;

namespace SoftFocusBackend.Tracking.Domain.Services;

public interface IEmotionalCalendarCommandService
{
    Task<EmotionalCalendar?> HandleCreateEmotionalCalendarEntryAsync(CreateEmotionalCalendarEntryCommand command);
    Task<EmotionalCalendar?> HandleUpdateEmotionalCalendarEntryAsync(UpdateEmotionalCalendarEntryCommand command);
    Task<bool> HandleDeleteEmotionalCalendarEntryAsync(DeleteEmotionalCalendarEntryCommand command);
}