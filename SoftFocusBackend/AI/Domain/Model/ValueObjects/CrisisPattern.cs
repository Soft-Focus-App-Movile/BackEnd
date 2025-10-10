namespace SoftFocusBackend.AI.Domain.Model.ValueObjects;

public record CrisisPattern
{
    public CrisisSeverity Severity { get; init; }
    public string TriggerReason { get; init; }
    public DateTime DetectedAt { get; init; }

    public CrisisPattern(CrisisSeverity severity, string triggerReason)
    {
        if (string.IsNullOrWhiteSpace(triggerReason))
            throw new ArgumentException("TriggerReason is required", nameof(triggerReason));

        Severity = severity;
        TriggerReason = triggerReason;
        DetectedAt = DateTime.UtcNow;
    }

    public bool RequiresImmediateAttention() => Severity == CrisisSeverity.Critical;
    public bool RequiresProfessionalReview() => Severity is CrisisSeverity.High or CrisisSeverity.Critical;

    public string GetSeverityString() => Severity switch
    {
        CrisisSeverity.Low => "low",
        CrisisSeverity.Moderate => "moderate",
        CrisisSeverity.High => "high",
        CrisisSeverity.Critical => "critical",
        _ => "unknown"
    };
}

public enum CrisisSeverity
{
    Low,
    Moderate,
    High,
    Critical
}
