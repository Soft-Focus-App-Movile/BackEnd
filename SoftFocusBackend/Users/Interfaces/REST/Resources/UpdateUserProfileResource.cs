using System.ComponentModel.DataAnnotations;

namespace SoftFocusBackend.Users.Interfaces.REST.Resources;

public record UpdateUserProfileResource
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters")]
    public string FullName { get; init; } = string.Empty;

    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string? FirstName { get; init; }

    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string? LastName { get; init; }

    public DateTime? DateOfBirth { get; init; }

    [RegularExpression("^(Male|Female|Other|PreferNotToSay)$", ErrorMessage = "Invalid gender value")]
    public string? Gender { get; init; }

    [Phone(ErrorMessage = "Invalid phone number format")]
    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
    public string? Phone { get; init; }

    [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters")]
    public string? Bio { get; init; }

    [StringLength(50, ErrorMessage = "Country cannot exceed 50 characters")]
    public string? Country { get; init; }

    [StringLength(50, ErrorMessage = "City cannot exceed 50 characters")]
    public string? City { get; init; }

    public List<string>? Interests { get; init; }

    public List<string>? MentalHealthGoals { get; init; }

    public bool? EmailNotifications { get; init; }

    public bool? PushNotifications { get; init; }

    public bool? IsProfilePublic { get; init; }


    [Url(ErrorMessage = "Invalid profile image URL")]
    public string? ProfileImageUrl { get; init; }


    public IFormFile? ProfileImage { get; init; }
}