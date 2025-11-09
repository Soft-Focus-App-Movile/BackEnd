using MongoDB.Bson.Serialization.Attributes;
using SoftFocusBackend.Crisis.Domain.Model.ValueObjects;
using SoftFocusBackend.Shared.Domain.Entities;

namespace SoftFocusBackend.Crisis.Domain.Model.Aggregates;

public class CrisisAlert : BaseEntity
{
    [BsonElement("patientId")]
    public string PatientId { get; set; }

    [BsonElement("psychologistId")]
    public string PsychologistId { get; set; }

    [BsonElement("severity")]
    public AlertSeverity Severity { get; set; }

    [BsonElement("status")]
    public AlertStatus Status { get; set; }

    [BsonElement("triggerSource")]
    public string TriggerSource { get; set; }

    [BsonElement("triggerReason")]
    public string? TriggerReason { get; set; }

    [BsonElement("location")]
    public Location? Location { get; set; }

    [BsonElement("emotionalContext")]
    public EmotionalContext? EmotionalContext { get; set; }

    [BsonElement("psychologistNotes")]
    public string? PsychologistNotes { get; set; }

    [BsonElement("attendedAt")]
    public DateTime? AttendedAt { get; set; }

    [BsonElement("resolvedAt")]
    public DateTime? ResolvedAt { get; set; }

    public CrisisAlert() { }

    public CrisisAlert(
        string patientId,
        string psychologistId,
        AlertSeverity severity,
        string triggerSource,
        string? triggerReason = null,
        Location? location = null,
        EmotionalContext? emotionalContext = null)
    {
        PatientId = patientId;
        PsychologistId = psychologistId;
        Severity = severity;
        Status = AlertStatus.Pending;
        TriggerSource = triggerSource;
        TriggerReason = triggerReason;
        Location = location;
        EmotionalContext = emotionalContext;
    }

    public void MarkAsAttended(string? notes = null)
    {
        Status = AlertStatus.Attended;
        AttendedAt = DateTime.UtcNow;
        PsychologistNotes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsResolved(string? notes = null)
    {
        Status = AlertStatus.Resolved;
        ResolvedAt = DateTime.UtcNow;
        if (notes != null)
        {
            PsychologistNotes = notes;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public void Dismiss(string? notes = null)
    {
        Status = AlertStatus.Dismissed;
        if (notes != null)
        {
            PsychologistNotes = notes;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateSeverity(AlertSeverity newSeverity)
    {
        Severity = newSeverity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddNotes(string notes)
    {
        PsychologistNotes = string.IsNullOrWhiteSpace(PsychologistNotes)
            ? notes
            : $"{PsychologistNotes}\n---\n{notes}";
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsCritical() => Severity == AlertSeverity.Critical;
    public bool IsPending() => Status == AlertStatus.Pending;
    public bool RequiresImmediateAttention() => IsCritical() && IsPending();
}
