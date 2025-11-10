using SoftFocusBackend.Shared.Domain.Events;

namespace SoftFocusBackend.Tracking.Domain.Model.Events;

/// <summary>
/// Evento: Un paciente completó su check-in diario
/// </summary>
public class CheckInCompletedEvent : DomainEvent
{
    public string CheckInId { get; }
    public string PatientId { get; }
    public int EmotionalLevel { get; }
    public int EnergyLevel { get; }
    public List<string> Symptoms { get; }
    public bool IsCritical { get; }

    public CheckInCompletedEvent(
        string checkInId,
        string patientId,
        int emotionalLevel,
        int energyLevel,
        List<string> symptoms)
    {
        CheckInId = checkInId;
        PatientId = patientId;
        EmotionalLevel = emotionalLevel;
        EnergyLevel = energyLevel;
        Symptoms = symptoms ?? new List<string>();
        IsCritical = emotionalLevel <= 3 || energyLevel <= 3;
    }
}