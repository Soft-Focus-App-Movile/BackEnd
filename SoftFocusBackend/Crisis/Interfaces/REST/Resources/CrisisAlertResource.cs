namespace SoftFocusBackend.Crisis.Interfaces.REST.Resources;

public record CrisisAlertResource(
    string Id,
    string PatientId,
    string PatientName,
    string? PatientPhotoUrl,
    string PsychologistId,
    string Severity,
    string Status,
    string TriggerSource,
    string? TriggerReason,
    LocationResource? Location,
    EmotionalContextResource? EmotionalContext,
    string? PsychologistNotes,
    DateTime CreatedAt,
    DateTime? AttendedAt,
    DateTime? ResolvedAt
);

public record LocationResource(
    double? Latitude,
    double? Longitude,
    string DisplayString
);

public record EmotionalContextResource(
    string? LastDetectedEmotion,
    DateTime? LastEmotionDetectedAt,
    string? EmotionSource
);
