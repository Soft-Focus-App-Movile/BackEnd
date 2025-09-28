using System.ComponentModel.DataAnnotations;

namespace SoftFocusBackend.Auth.Domain.Model.ValueObjects;

public record LoginCredentials
{
    [Required]
    [EmailAddress]
    public string Email { get; init; }
    
    [Required]
    [MinLength(1)]
    public string Password { get; init; }

    public LoginCredentials(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty.", nameof(email));
        
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));

        Email = email.Trim().ToLowerInvariant();
        Password = password;
    }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Email) && 
               !string.IsNullOrWhiteSpace(Password) &&
               Email.Contains('@') &&
               Email.Length >= 5;
    }
}