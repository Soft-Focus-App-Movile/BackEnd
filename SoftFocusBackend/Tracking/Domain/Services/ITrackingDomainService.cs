using SoftFocusBackend.Tracking.Domain.Model.Aggregates;

namespace SoftFocusBackend.Tracking.Domain.Services;

public interface ITrackingDomainService
{
    Task<bool> HasUserCompletedCheckInTodayAsync(string userId);
    Task<bool> HasUserEmotionalCalendarEntryForDateAsync(string userId, DateTime date);
    Task<CheckIn> CreateCheckInAsync(string userId, int emotionalLevel, int energyLevel, 
        string moodDescription, decimal sleepHours, List<string> symptoms, string notes);
    Task<EmotionalCalendar> CreateEmotionalCalendarEntryAsync(string userId, DateTime date, 
        string emotionalEmoji, int moodLevel, List<string> emotionalTags);
    Task<bool> CanCheckInBeDeletedAsync(string checkInId);
    Task<bool> CanEmotionalCalendarEntryBeDeletedAsync(string entryId);
}