namespace SoftFocusBackend.Subscription.Application.Commands;

public class UpgradeToProCommand
{
    public string UserId { get; set; } = string.Empty;
    public string StripeCustomerId { get; set; } = string.Empty;
    public string StripeSubscriptionId { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}
