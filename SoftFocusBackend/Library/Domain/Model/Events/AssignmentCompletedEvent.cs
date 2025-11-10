using SoftFocusBackend.Shared.Domain.Events;

namespace SoftFocusBackend.Library.Domain.Model.Events;

/// <summary>
/// Evento: Un paciente completó una asignación
/// </summary>
public class AssignmentCompletedEvent : DomainEvent
{
    public string AssignmentId { get; }
    public string PatientId { get; }
    public string PsychologistId { get; }
    public string ContentTitle { get; }
    public string ContentType { get; }

    public AssignmentCompletedEvent(
        string assignmentId,
        string patientId,
        string psychologistId,
        string contentTitle,
        string contentType)
    {
        AssignmentId = assignmentId;
        PatientId = patientId;
        PsychologistId = psychologistId;
        ContentTitle = contentTitle;
        ContentType = contentType;
    }
}