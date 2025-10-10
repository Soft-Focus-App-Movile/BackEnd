using System.ComponentModel.DataAnnotations;

namespace SoftFocusBackend.Auth.Interfaces.REST.Resources;

public record OAuthVerifyResource
{
    [Required(ErrorMessage = "Provider is required")]
    [RegularExpression("^(Google|Facebook)$", ErrorMessage = "Provider must be either 'Google' or 'Facebook'")]
    public string Provider { get; init; } = string.Empty;

    [Required(ErrorMessage = "Access token is required")]
    public string AccessToken { get; init; } = string.Empty;

    public string? RefreshToken { get; init; }

    public DateTime? ExpiresAt { get; init; }
}
