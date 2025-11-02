using SoftFocusBackend.Subscription.Domain.ValueObjects;

namespace SoftFocusBackend.Subscription.Application.Queries;

public class CheckFeatureAccessQuery
{
    public string UserId { get; set; } = string.Empty;
    public FeatureType FeatureType { get; set; }
}
