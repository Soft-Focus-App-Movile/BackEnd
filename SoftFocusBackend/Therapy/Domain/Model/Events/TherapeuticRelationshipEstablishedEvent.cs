using SoftFocusBackend.Shared.Domain.Events;

namespace SoftFocusBackend.Therapy.Domain.Model.Events;

/// <summary>
/// Evento: Se estableció una nueva relación terapéutica
/// </summary>
public class TherapeuticRelationshipEstablishedEvent : DomainEvent
{
    public string RelationshipId { get; }
    public string PsychologistId { get; }
    public string PatientId { get; }

    public TherapeuticRelationshipEstablishedEvent(
        string relationshipId,
        string psychologistId,
        string patientId)
    {
        RelationshipId = relationshipId;
        PsychologistId = psychologistId;
        PatientId = patientId;
    }
}