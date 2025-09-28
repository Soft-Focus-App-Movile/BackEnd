using System.ComponentModel.DataAnnotations;

namespace SoftFocusBackend.Auth.Interfaces.REST.Resources;

public record OAuthSignInResource
{
    [Required(ErrorMessage = "Provider is required")]
    [StringLength(50, ErrorMessage = "Provider name cannot exceed 50 characters")]
    public string Provider { get; init; } = string.Empty;

    [Required(ErrorMessage = "Access token is required")]
    [StringLength(2000, ErrorMessage = "Access token cannot exceed 2000 characters")]
    public string AccessToken { get; init; } = string.Empty;

    public string? RefreshToken { get; init; }
    public DateTime? ExpiresAt { get; init; }
}