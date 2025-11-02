namespace SoftFocusBackend.Subscription.Application.DTOs;

public class CheckoutSessionResponse
{
    public string SessionId { get; set; } = string.Empty;
    public string CheckoutUrl { get; set; } = string.Empty;
}
