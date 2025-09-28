using System.Security.Claims;

namespace SoftFocusBackend.Auth.Domain.Model.ValueObjects;

public record AuthToken
{
    public string Token { get; init; }
    public DateTime ExpiresAt { get; init; }
    public DateTime IssuedAt { get; init; }
    public string TokenType { get; init; }
    public IReadOnlyList<Claim> Claims { get; init; }

    public AuthToken(string token, DateTime expiresAt, DateTime issuedAt, IEnumerable<Claim> claims, string tokenType = "Bearer")
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be null or empty.", nameof(token));
        
        if (expiresAt <= issuedAt)
            throw new ArgumentException("ExpiresAt must be greater than IssuedAt.", nameof(expiresAt));
        
        if (claims == null)
            throw new ArgumentNullException(nameof(claims));

        Token = token;
        ExpiresAt = expiresAt;
        IssuedAt = issuedAt;
        TokenType = tokenType;
        Claims = claims.ToList().AsReadOnly();
    }

    public static AuthToken CreateFromUser(AuthenticatedUser user, string jwtToken, DateTime expiresAt, DateTime issuedAt)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
            new("user_id", user.Id),
            new("full_name", user.FullName),
            new("email", user.Email),
            new("role", user.Role)
        };

        if (!string.IsNullOrWhiteSpace(user.ProfileImageUrl))
        {
            claims.Add(new Claim("profile_image", user.ProfileImageUrl));
        }

        if (user.LastLogin.HasValue)
        {
            claims.Add(new Claim("last_login", user.LastLogin.Value.ToString("O")));
        }

        return new AuthToken(jwtToken, expiresAt, issuedAt, claims);
    }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    public bool IsValid => !IsExpired && !string.IsNullOrWhiteSpace(Token);
    public TimeSpan TimeUntilExpiration => ExpiresAt - DateTime.UtcNow;
    
    public string GetClaimValue(string claimType)
    {
        return Claims.FirstOrDefault(c => c.Type == claimType)?.Value ?? string.Empty;
    }
    
    public bool HasClaim(string claimType)
    {
        return Claims.Any(c => c.Type == claimType);
    }
    
    public string GetUserId()
    {
        return GetClaimValue("user_id");
    }
    
    public string GetUserRole()
    {
        return GetClaimValue("role");
    }
    
    public string GetUserEmail()
    {
        return GetClaimValue("email");
    }
}