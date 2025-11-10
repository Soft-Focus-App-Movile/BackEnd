using SoftFocusBackend.Shared.Domain.Events;

namespace SoftFocusBackend.Therapy.Domain.Model.Events;

/// <summary>
/// Evento: Se terminó una relación terapéutica
/// </summary>
public class TherapeuticRelationshipTerminatedEvent : DomainEvent
{
    public string RelationshipId { get; }
    public string PsychologistId { get; }
    public string PatientId { get; }

    public TherapeuticRelationshipTerminatedEvent(
        string relationshipId,
        string psychologistId,
        string patientId)
    {
        RelationshipId = relationshipId;
        PsychologistId = psychologistId;
        PatientId = patientId;
    }
}