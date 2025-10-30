namespace SoftFocusBackend.Auth.Domain.Model.ValueObjects;

public record AuthenticatedUser
{
    public string Id { get; init; }
    public string FullName { get; init; }
    public string Email { get; init; }
    public string Role { get; init; }
    public string? ProfileImageUrl { get; init; }
    public DateTime? LastLogin { get; init; }
    public bool? IsVerified { get; init; }

    public AuthenticatedUser(string id, string fullName, string email, string role, string? profileImageUrl = null, DateTime? lastLogin = null, bool? isVerified = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Id cannot be null or empty.", nameof(id));
        
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("FullName cannot be null or empty.", nameof(fullName));
        
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty.", nameof(email));
        
        if (string.IsNullOrWhiteSpace(role))
            throw new ArgumentException("Role cannot be null or empty.", nameof(role));

        Id = id;
        FullName = fullName.Trim();
        Email = email.Trim().ToLowerInvariant();
        Role = role.Trim();
        ProfileImageUrl = profileImageUrl?.Trim();
        LastLogin = lastLogin;
        IsVerified = isVerified;
    }

    public bool IsAdmin() => Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
    public bool IsPsychologist() => Role.Equals("Psychologist", StringComparison.OrdinalIgnoreCase);
    public bool IsGeneral() => Role.Equals("General", StringComparison.OrdinalIgnoreCase);
    
    public bool HasRole(string roleName) => Role.Equals(roleName, StringComparison.OrdinalIgnoreCase);
    
    public bool CanManageUsers() => IsAdmin();
    public bool CanProvideTherapy() => IsPsychologist();
    public bool CanAccessPremiumFeatures() => IsPsychologist() || IsAdmin();
    
    public bool IsActive => true;
    
    public string GetDisplayRole() => Role switch
    {
        "Admin" => "Admin",
        "Psychologist" => "Psychologist",
        "General" => "General user", 
        _ => Role
    };
}