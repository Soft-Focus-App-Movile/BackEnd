using MongoDB.Bson.Serialization.Attributes;
using SoftFocusBackend.Shared.Domain.Entities;
using SoftFocusBackend.Users.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Users.Domain.Model.Aggregates;

public class User : BaseEntity
{
    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("passwordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    [BsonElement("userType")]
    public UserType UserType { get; set; }

    [BsonElement("fullName")]
    public string FullName { get; set; } = string.Empty;

    [BsonElement("firstName")]
    public string? FirstName { get; set; }

    [BsonElement("lastName")]
    public string? LastName { get; set; }

    [BsonElement("dateOfBirth")]
    public DateTime? DateOfBirth { get; set; }

    [BsonElement("gender")]
    public string? Gender { get; set; }

    [BsonElement("phone")]
    public string? Phone { get; set; }

    [BsonElement("profileImageUrl")]
    public string? ProfileImageUrl { get; set; }

    [BsonElement("bio")]
    public string? Bio { get; set; }

    [BsonElement("country")]
    public string? Country { get; set; }

    [BsonElement("city")]
    public string? City { get; set; }

    [BsonElement("interests")]
    public List<string>? Interests { get; set; }

    [BsonElement("mentalHealthGoals")]
    public List<string>? MentalHealthGoals { get; set; }

    [BsonElement("emailNotifications")]
    public bool EmailNotifications { get; set; } = true;

    [BsonElement("pushNotifications")]
    public bool PushNotifications { get; set; } = true;

    [BsonElement("isProfilePublic")]
    public bool IsProfilePublic { get; set; } = false;

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("lastLogin")]
    public DateTime? LastLogin { get; set; }

    public bool IsGeneral() => UserType == UserType.General;
    public bool IsPsychologist() => UserType == UserType.Psychologist;
    public bool IsAdmin() => UserType == UserType.Admin;

    public void UpdateLastLogin()
    {
        LastLogin = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateProfile(string fullName, string? firstName = null, string? lastName = null,
        DateTime? dateOfBirth = null, string? gender = null, string? phone = null,
        string? bio = null, string? country = null, string? city = null,
        List<string>? interests = null, List<string>? mentalHealthGoals = null)
    {
        // Always update FullName (calculated from FirstName + LastName)
        FullName = fullName;

        // Only update fields that are explicitly provided (not null)
        if (firstName != null) FirstName = firstName;
        if (lastName != null) LastName = lastName;
        if (dateOfBirth.HasValue) DateOfBirth = dateOfBirth;
        if (gender != null) Gender = gender;
        if (phone != null) Phone = phone;
        if (bio != null) Bio = bio;
        if (country != null) Country = country;
        if (city != null) City = city;
        if (interests != null) Interests = interests;
        if (mentalHealthGoals != null) MentalHealthGoals = mentalHealthGoals;

        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateNotificationSettings(bool emailNotifications, bool pushNotifications)
    {
        EmailNotifications = emailNotifications;
        PushNotifications = pushNotifications;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetProfileImageUrl(string? imageUrl)
    {
        ProfileImageUrl = imageUrl;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePassword(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty");

        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public virtual void ValidateForCreation()
    {
        if (string.IsNullOrWhiteSpace(Email))
            throw new ArgumentException("Email is required");

        if (string.IsNullOrWhiteSpace(PasswordHash))
            throw new ArgumentException("Password hash is required");

        if (string.IsNullOrWhiteSpace(FullName))
            throw new ArgumentException("Full name is required");

        if (!Email.Contains('@'))
            throw new ArgumentException("Invalid email format");
    }
}

