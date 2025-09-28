using System.ComponentModel.DataAnnotations;

namespace SoftFocusBackend.Auth.Interfaces.REST.Resources;

public record RegisterResource
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&]).+$", 
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number and one special character")]
    public string Password { get; init; } = string.Empty;

    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters")]
    public string FullName { get; init; } = string.Empty;

    [Required(ErrorMessage = "User type is required")]
    [RegularExpression("^(General|Psychologist)$", ErrorMessage = "User type must be either 'General' or 'Psychologist'")]
    public string UserType { get; init; } = string.Empty;

    public string? ProfessionalLicense { get; init; }
    public string[]? Specialties { get; init; }
}