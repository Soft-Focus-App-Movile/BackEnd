namespace SoftFocusBackend.Subscription.Application.Commands;

public class 
    
    
    CancelSubscriptionCommand
{
    public string UserId { get; set; } = string.Empty;
    public bool CancelImmediately { get; set; } = false;
}
