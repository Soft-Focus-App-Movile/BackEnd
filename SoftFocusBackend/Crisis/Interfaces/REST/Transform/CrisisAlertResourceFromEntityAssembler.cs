using SoftFocusBackend.Crisis.Domain.Model.Aggregates;
using SoftFocusBackend.Crisis.Interfaces.REST.Resources;
using SoftFocusBackend.Users.Application.Internal.OutboundServices;

namespace SoftFocusBackend.Crisis.Interfaces.REST.Transform;

public class CrisisAlertResourceFromEntityAssembler
{
    private readonly IUserFacade _userFacade;

    public CrisisAlertResourceFromEntityAssembler(IUserFacade userFacade)
    {
        _userFacade = userFacade;
    }

    public async Task<CrisisAlertResource> ToResourceFromEntity(CrisisAlert entity)
    {
        var patient = await _userFacade.GetUserByIdAsync(entity.PatientId);

        return new CrisisAlertResource(
            Id: entity.Id,
            PatientId: entity.PatientId,
            PatientName: patient?.FullName ?? "Paciente desconocido",
            PatientPhotoUrl: patient?.ProfileImageUrl,
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

    public async Task<IEnumerable<CrisisAlertResource>> ToResourceFromEntityList(IEnumerable<CrisisAlert> entities)
    {
        var tasks = entities.Select(ToResourceFromEntity);
        return await Task.WhenAll(tasks);
    }
}
