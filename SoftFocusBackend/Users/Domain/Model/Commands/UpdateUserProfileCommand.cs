using SoftFocusBackend.Users.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Users.Domain.Model.Commands;

public record UpdateUserProfileCommand
{
    public string UserId { get; init; }
    public string FullName { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public DateTime? DateOfBirth { get; init; }
    public string? Gender { get; init; }
    public string? Phone { get; init; }
    public string? Bio { get; init; }
    public string? Country { get; init; }
    public string? City { get; init; }
    public List<string>? Interests { get; init; }
    public List<string>? MentalHealthGoals { get; init; }
    public bool? EmailNotifications { get; init; }
    public bool? PushNotifications { get; init; }
    public bool? IsProfilePublic { get; init; }
    public string? ProfileImageUrl { get; init; }
    public DateTime RequestedAt { get; init; }
    public byte[]? ProfileImageBytes { get; init; }
    public string? ProfileImageFileName { get; init; }
    public UpdateUserProfileCommand(string userId, string fullName, string? firstName = null,
        string? lastName = null, DateTime? dateOfBirth = null, string? gender = null,
        string? phone = null, string? bio = null, string? country = null, string? city = null,
        List<string>? interests = null, List<string>? mentalHealthGoals = null,
        bool? emailNotifications = null, bool? pushNotifications = null,
        bool? isProfilePublic = null, string? profileImageUrl = null,
        byte[]? profileImageBytes = null, string? profileImageFileName = null)
    {
        UserId = userId ?? throw new ArgumentNullException(nameof(userId));
        FullName = fullName?.Trim() ?? throw new ArgumentNullException(nameof(fullName));
        FirstName = firstName?.Trim();
        LastName = lastName?.Trim();
        DateOfBirth = dateOfBirth;
        Gender = gender?.Trim();
        Phone = phone?.Trim();
        Bio = bio?.Trim();
        Country = country?.Trim();
        City = city?.Trim();
        Interests = interests;
        MentalHealthGoals = mentalHealthGoals;
        EmailNotifications = emailNotifications;
        PushNotifications = pushNotifications;
        IsProfilePublic = isProfilePublic;
        ProfileImageUrl = profileImageUrl?.Trim();
        ProfileImageBytes = profileImageBytes;
        ProfileImageFileName = profileImageFileName;
        RequestedAt = DateTime.UtcNow;
    }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(UserId) &&
               !string.IsNullOrWhiteSpace(FullName);
    }

    public string GetAuditString()
    {
        return $"UserId: {UserId} | FullName: {FullName} | RequestedAt: {RequestedAt:yyyy-MM-dd HH:mm:ss} UTC";
    }
}