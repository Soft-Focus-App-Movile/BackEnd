namespace SoftFocusBackend.AI.Application.ACL.Services;

public interface ICrisisIntegrationService
{
    Task TriggerCrisisAlertAsync(CrisisAlertRequest request);
}

public record CrisisAlertRequest
{
    public string UserId { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
    public string TriggerReason { get; init; } = string.Empty;
    public string Context { get; init; } = string.Empty;
    public DateTime DetectedAt { get; init; }
}
