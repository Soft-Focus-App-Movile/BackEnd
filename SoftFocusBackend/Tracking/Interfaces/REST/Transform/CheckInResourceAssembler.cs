using SoftFocusBackend.Tracking.Domain.Model.Aggregates;
using SoftFocusBackend.Tracking.Domain.Model.Commands;
using SoftFocusBackend.Tracking.Interfaces.REST.Resources;

namespace SoftFocusBackend.Tracking.Interfaces.REST.Transform;

public static class CheckInResourceAssembler
{
    public static CheckInResource ToResource(CheckIn checkIn)
    {
        return new CheckInResource
        {
            Id = checkIn.Id,
            UserId = checkIn.UserId,
            EmotionalLevel = checkIn.EmotionalLevel,
            EnergyLevel = checkIn.EnergyLevel,
            MoodDescription = checkIn.MoodDescription,
            SleepHours = checkIn.SleepHours,
            Symptoms = checkIn.Symptoms,
            Notes = checkIn.Notes,
            CompletedAt = checkIn.CompletedAt,
            CreatedAt = checkIn.CreatedAt,
            UpdatedAt = checkIn.UpdatedAt
        };
    }

    public static List<CheckInResource> ToResourceList(List<CheckIn> checkIns)
    {
        return checkIns.Select(ToResource).ToList();
    }

    public static CreateCheckInCommand ToCreateCommand(CreateCheckInResource resource, string userId)
    {
        return new CreateCheckInCommand(
            userId: userId,
            emotionalLevel: resource.EmotionalLevel,
            energyLevel: resource.EnergyLevel,
            moodDescription: resource.MoodDescription,
            sleepHours: resource.SleepHours,
            symptoms: resource.Symptoms,
            notes: resource.Notes
        );
    }

    public static UpdateCheckInCommand ToUpdateCommand(UpdateCheckInResource resource, string checkInId)
    {
        return new UpdateCheckInCommand(
            checkInId: checkInId,
            emotionalLevel: resource.EmotionalLevel,
            energyLevel: resource.EnergyLevel,
            moodDescription: resource.MoodDescription,
            sleepHours: resource.SleepHours,
            symptoms: resource.Symptoms,
            notes: resource.Notes
        );
    }

    public static object ToSuccessResponse(CheckInResource checkIn, string message = "Check-in processed successfully")
    {
        return new
        {
            success = true,
            message,
            data = checkIn,
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