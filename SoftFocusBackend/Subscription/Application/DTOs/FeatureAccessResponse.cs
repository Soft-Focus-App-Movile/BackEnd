namespace SoftFocusBackend.Subscription.Application.DTOs;

public class FeatureAccessResponse
{
    public bool HasAccess { get; set; }
    public string? DenialReason { get; set; }
    public int? CurrentUsage { get; set; }
    public int? Limit { get; set; }
    public string? UpgradeMessage { get; set; }
}
