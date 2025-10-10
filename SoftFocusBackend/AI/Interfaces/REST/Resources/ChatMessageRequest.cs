using System.ComponentModel.DataAnnotations;

namespace SoftFocusBackend.AI.Interfaces.REST.Resources;

public record ChatMessageRequest
{
    [Required(ErrorMessage = "Message is required")]
    [MaxLength(2000, ErrorMessage = "Message cannot exceed 2000 characters")]
    public string Message { get; init; } = string.Empty;

    public string? SessionId { get; init; }
}
