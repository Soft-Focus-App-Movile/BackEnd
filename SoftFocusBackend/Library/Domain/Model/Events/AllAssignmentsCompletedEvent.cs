using SoftFocusBackend.Shared.Domain.Events;

namespace SoftFocusBackend.Library.Domain.Model.Events;

/// <summary>
/// Evento: Un paciente completó TODAS sus asignaciones pendientes
/// </summary>
public class AllAssignmentsCompletedEvent : DomainEvent
{
    public string PatientId { get; }
    public string PsychologistId { get; }
    public int CompletedCount { get; }

    public AllAssignmentsCompletedEvent(
        string patientId,
        string psychologistId,
        int completedCount)
    {
        PatientId = patientId;
        PsychologistId = psychologistId;
        CompletedCount = completedCount;
    }
}