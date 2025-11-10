using SoftFocusBackend.Shared.Domain.Events;

namespace SoftFocusBackend.Library.Domain.Model.Events;

/// <summary>
/// Evento: Se asignó contenido a un paciente
/// </summary>
public class ContentAssignedEvent : DomainEvent
{
    public string AssignmentId { get; }
    public string PsychologistId { get; }
    public string PatientId { get; }
    public string ContentId { get; }
    public string ContentType { get; }
    public string ContentTitle { get; }
    public string Notes { get; }

    public ContentAssignedEvent(
        string assignmentId,
        string psychologistId,
        string patientId,
        string contentId,
        string contentType,
        string contentTitle,
        string notes)
    {
        AssignmentId = assignmentId;
        PsychologistId = psychologistId;
        PatientId = patientId;
        ContentId = contentId;
        ContentType = contentType;
        ContentTitle = contentTitle;
        Notes = notes;
    }
}