using SoftFocusBackend.Users.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Users.Interfaces.REST.Resources;

public record UserProfileResource
{
    public string Id { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public UserType UserType { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public string? Gender { get; init; }
    public string? Phone { get; init; }
    public string? ProfileImageUrl { get; init; }
    public string? Bio { get; init; }
    public string? Country { get; init; }
    public string? City { get; init; }
    public List<string>? Interests { get; init; }
    public List<string>? MentalHealthGoals { get; init; }
    public bool EmailNotifications { get; init; }
    public bool PushNotifications { get; init; }
    public bool IsProfilePublic { get; init; }
    public bool IsActive { get; init; }
    public DateTime? LastLogin { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}