using SoftFocusBackend.Crisis.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Crisis.Domain.Model.Commands;

public record CreateCrisisAlertCommand(
    string PatientId,
    AlertSeverity Severity,
    string TriggerSource,
    string? TriggerReason = null,
    double? Latitude = null,
    double? Longitude = null,
    string? LastDetectedEmotion = null,
    DateTime? LastEmotionDetectedAt = null,
    string? EmotionSource = null
);
