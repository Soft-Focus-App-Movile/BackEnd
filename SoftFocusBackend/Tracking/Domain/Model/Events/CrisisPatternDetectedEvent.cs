using SoftFocusBackend.Shared.Domain.Events;

namespace SoftFocusBackend.Tracking.Domain.Model.Events;

/// <summary>
/// Evento: Se detectó un patrón de crisis (múltiples check-ins bajos)
/// </summary>
public class CrisisPatternDetectedEvent : DomainEvent
{
    public string PatientId { get; }
    public int LowEmotionalDaysCount { get; }
    public string Reason { get; }

    public CrisisPatternDetectedEvent(
        string patientId,
        int lowEmotionalDaysCount,
        string reason)
    {
        PatientId = patientId;
        LowEmotionalDaysCount = lowEmotionalDaysCount;
        Reason = reason;
    }
}