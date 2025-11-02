using SoftFocusBackend.Subscription.Domain.ValueObjects;

namespace SoftFocusBackend.Subscription.Application.Commands;

public class TrackFeatureUsageCommand
{
    public string UserId { get; set; } = string.Empty;
    public FeatureType FeatureType { get; set; }
}
