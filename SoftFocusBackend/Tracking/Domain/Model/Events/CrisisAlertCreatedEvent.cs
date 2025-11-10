using SoftFocusBackend.Shared.Domain.Events;

namespace SoftFocusBackend.Crisis.Domain.Model.Events;

/// <summary>
/// Evento: Se creó una alerta de crisis
/// </summary>
public class CrisisAlertCreatedEvent : DomainEvent
{
    public string AlertId { get; }
    public string PatientId { get; }
    public string PsychologistId { get; }
    public string Severity { get; }
    public string TriggerSource { get; }
    public string TriggerReason { get; }

    public CrisisAlertCreatedEvent(
        string alertId,
        string patientId,
        string psychologistId,
        string severity,
        string triggerSource,
        string triggerReason)
    {
        AlertId = alertId;
        PatientId = patientId;
        PsychologistId = psychologistId;
        Severity = severity;
        TriggerSource = triggerSource;
        TriggerReason = triggerReason;
    }
}