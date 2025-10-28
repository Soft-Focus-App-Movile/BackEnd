namespace SoftFocusBackend.Auth.Interfaces.REST.Resources;

public record AuthenticatedUserResource
{
    public string Id { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string? ProfileImageUrl { get; init; }
    public DateTime? LastLogin { get; init; }
    public string RoleDisplay { get; init; } = string.Empty;
    public UserCapabilitiesResource Capabilities { get; init; } = new();
    public bool? IsVerified { get; init; }
}

public record UserCapabilitiesResource
{
    public bool CanManageUsers { get; init; }
    public bool CanProvideTherapy { get; init; }
    public bool CanAccessPremiumFeatures { get; init; }
    public bool IsAdmin { get; init; }
    public bool IsPsychologist { get; init; }
    public bool IsGeneral { get; init; }
}