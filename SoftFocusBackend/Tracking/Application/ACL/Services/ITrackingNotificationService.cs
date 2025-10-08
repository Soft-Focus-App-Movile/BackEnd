namespace SoftFocusBackend.Tracking.Application.ACL.Services;

public interface ITrackingNotificationService
{
    Task NotifyCheckInCompletedAsync(string userId, string checkInId, int emotionalLevel, int energyLevel);
    Task NotifyEmotionalCalendarEntryCreatedAsync(string userId, string entryId, DateTime date, int moodLevel);
    Task NotifyUserEngagementAsync(string userId, string activityType);
}