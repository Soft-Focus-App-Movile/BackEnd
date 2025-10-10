using SoftFocusBackend.Tracking.Domain.Model.Aggregates;
using SoftFocusBackend.Tracking.Domain.Model.Commands;
using SoftFocusBackend.Tracking.Interfaces.REST.Resources;

namespace SoftFocusBackend.Tracking.Interfaces.REST.Transform;

public static class EmotionalCalendarResourceAssembler
{
    public static EmotionalCalendarResource ToResource(EmotionalCalendar entry)
    {
        return new EmotionalCalendarResource
        {
            Id = entry.Id,
            UserId = entry.UserId,
            Date = entry.Date,
            EmotionalEmoji = entry.EmotionalEmoji,
            MoodLevel = entry.MoodLevel,
            EmotionalTags = entry.EmotionalTags,
            CreatedAt = entry.CreatedAt,
            UpdatedAt = entry.UpdatedAt
        };
    }

    public static List<EmotionalCalendarResource> ToResourceList(List<EmotionalCalendar> entries)
    {
        return entries.Select(ToResource).ToList();
    }

    public static CreateEmotionalCalendarEntryCommand ToCreateCommand(CreateEmotionalCalendarEntryResource resource, string userId)
    {
        return new CreateEmotionalCalendarEntryCommand(
            userId: userId,
            date: resource.Date,
            emotionalEmoji: resource.EmotionalEmoji,
            moodLevel: resource.MoodLevel,
            emotionalTags: resource.EmotionalTags
        );
    }

    public static UpdateEmotionalCalendarEntryCommand ToUpdateCommand(UpdateEmotionalCalendarEntryResource resource, string entryId)
    {
        return new UpdateEmotionalCalendarEntryCommand(
            entryId: entryId,
            emotionalEmoji: resource.EmotionalEmoji,
            moodLevel: resource.MoodLevel,
            emotionalTags: resource.EmotionalTags
        );
    }

    public static object ToCalendarResponse(List<EmotionalCalendarResource> entries, DateTime? startDate = null, DateTime? endDate = null)
    {
        return new
        {
            success = true,
            data = new
            {
                entries,
                totalCount = entries.Count,
                dateRange = new
                {
                    startDate = startDate?.ToString("yyyy-MM-dd"),
                    endDate = endDate?.ToString("yyyy-MM-dd")
                }
            },
            timestamp = DateTime.UtcNow
        };
    }

    public static object ToSuccessResponse(EmotionalCalendarResource entry, string message = "Emotional calendar entry processed successfully")
    {
        return new
        {
            success = true,
            message,
            data = entry,
            timestamp = DateTime.UtcNow
        };
    }

    public static object ToErrorResponse(string message, string? details = null)
    {
        return new
        {
            success = false,
            error = true,
            message,
            details,
            timestamp = DateTime.UtcNow
        };
    }
}