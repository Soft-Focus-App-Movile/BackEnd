using System.ComponentModel.DataAnnotations;

namespace SoftFocusBackend.Subscription.Application.DTOs;

public class CreateCheckoutSessionRequest
{
    [Required]
    public string SuccessUrl { get; set; } = string.Empty;

    [Required]
    public string CancelUrl { get; set; } = string.Empty;
}
