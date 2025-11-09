using SoftFocusBackend.Crisis.Domain.Model.Aggregates;
using SoftFocusBackend.Crisis.Interfaces.REST.Resources;

namespace SoftFocusBackend.Crisis.Interfaces.REST.Transform;

public static class CrisisAlertResourceFromEntityAssembler
{
    public static CrisisAlertResource ToResourceFromEntity(CrisisAlert entity)
    {
        return new CrisisAlertResource(
            Id: entity.Id,
            PatientId: entity.PatientId,
            PsychologistId: entity.PsychologistId,
            Severity: entity.Severity.ToString(),
            Status: entity.Status.ToString(),
            TriggerSource: entity.TriggerSource,
            TriggerReason: entity.TriggerReason,
            Location: entity.Location != null
                ? new LocationResource(
                    entity.Location.Latitude,
                    entity.Location.Longitude,
                    entity.Location.ToDisplayString())
                : null,
            EmotionalContext: entity.EmotionalContext != null && entity.EmotionalContext.HasEmotionalData()
                ? new EmotionalContextResource(
                    entity.EmotionalContext.LastDetectedEmotion,
                    entity.EmotionalContext.LastEmotionDetectedAt,
                    entity.EmotionalContext.EmotionSource)
                : null,
            PsychologistNotes: entity.PsychologistNotes,
            CreatedAt: entity.CreatedAt,
            AttendedAt: entity.AttendedAt,
            ResolvedAt: entity.ResolvedAt
        );
    }

    public static IEnumerable<CrisisAlertResource> ToResourceFromEntityList(IEnumerable<CrisisAlert> entities)
    {
        return entities.Select(ToResourceFromEntity);
    }
}
