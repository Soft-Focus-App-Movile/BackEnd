using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SoftFocusBackend.Auth.Domain.Model.ValueObjects;
using SoftFocusBackend.Auth.Infrastructure.Tokens.JWT.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace SoftFocusBackend.Auth.Infrastructure.Tokens.JWT.Services;

public class TokenService
{
    private readonly TokenSettings _tokenSettings;
    private readonly ILogger<TokenService> _logger;
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly SigningCredentials _signingCredentials;

    public TokenService(IOptions<TokenSettings> tokenSettings, ILogger<TokenService> logger)
    {
        _tokenSettings = tokenSettings?.Value ?? throw new ArgumentNullException(nameof(tokenSettings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (!_tokenSettings.IsValid())
        {
            var errors = _tokenSettings.GetValidationErrors();
            throw new InvalidOperationException($"Invalid TokenSettings: {string.Join(", ", errors)}");
        }

        _tokenHandler = new JwtSecurityTokenHandler();

        var key = Encoding.UTF8.GetBytes(_tokenSettings.SecretKey);
        _signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

        _logger.LogInformation("TokenService initialized with {ExpirationHours}h expiration",
            _tokenSettings.ExpirationHours);
    }

    public AuthToken GenerateToken(AuthenticatedUser user)
    {
        try
        {
            _logger.LogDebug("Generating token for user: {UserId} - {Email}", user.Id, user.Email);

            var now = DateTime.UtcNow;
            var expires = now.AddHours(_tokenSettings.ExpirationHours);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64),
                new(JwtRegisteredClaimNames.Email, user.Email),
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

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                NotBefore = now,
                IssuedAt = now,
                Issuer = _tokenSettings.Issuer,
                Audience = _tokenSettings.Audience,
                SigningCredentials = _signingCredentials
            };

            var token = _tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = _tokenHandler.WriteToken(token);

            _logger.LogInformation(
                "Token generated successfully for user: {UserId}, expires at: {Expires:yyyy-MM-dd HH:mm:ss} UTC",
                user.Id, expires);

            return AuthToken.CreateFromUser(user, tokenString, expires, now);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating token for user: {UserId}", user.Id);
            throw;
        }
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            _logger.LogDebug("Validating JWT token");

            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Token validation failed: token is null or empty");
                return null;
            }

            var key = Encoding.UTF8.GetBytes(_tokenSettings.SecretKey);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = _tokenSettings.ValidateIssuerSigningKey,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = _tokenSettings.ValidateIssuer,
                ValidIssuer = _tokenSettings.Issuer,
                ValidateAudience = _tokenSettings.ValidateAudience,
                ValidAudience = _tokenSettings.Audience,
                ValidateLifetime = _tokenSettings.ValidateLifetime,
                RequireExpirationTime = _tokenSettings.RequireExpirationTime,
                RequireSignedTokens = _tokenSettings.RequireSignedTokens,
                ClockSkew = TimeSpan.FromMinutes(_tokenSettings.ClockSkewMinutes)
            };

            var principal = _tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);

            _logger.LogDebug("Token validation successful");
            return principal;
        }
        catch (SecurityTokenExpiredException ex)
        {
            _logger.LogWarning("Token validation failed: token expired - {Message}", ex.Message);
            return null;
        }
        catch (SecurityTokenInvalidSignatureException ex)
        {
            _logger.LogWarning("Token validation failed: invalid signature - {Message}", ex.Message);
            return null;
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning("Token validation failed: {Message}", ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token validation");
            return null;
        }
    }

    public string GeneratePasswordResetToken(string userId, string email)
    {
        try
        {
            _logger.LogDebug("Generating password reset token for user: {UserId} - {Email}", userId, email);

            var now = DateTime.UtcNow;
            var expires = now.AddHours(1);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, userId),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64),
                new("user_id", userId),
                new("email", email),
                new("purpose", "password_reset")
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                NotBefore = now,
                IssuedAt = now,
                Issuer = _tokenSettings.Issuer,
                Audience = _tokenSettings.Audience,
                SigningCredentials = _signingCredentials
            };

            var token = _tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = _tokenHandler.WriteToken(token);

            _logger.LogInformation(
                "Password reset token generated for user: {UserId}, expires at: {Expires:yyyy-MM-dd HH:mm:ss} UTC",
                userId, expires);

            return tokenString;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating password reset token for user: {UserId}", userId);
            throw;
        }
    }

    public (bool IsValid, string UserId, string? Email) ValidatePasswordResetToken(string token)
    {
        try
        {
            _logger.LogDebug("Validating password reset token");

            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("Password reset token validation failed: token is null or empty");
                return (false, string.Empty, null);
            }

            var key = Encoding.UTF8.GetBytes(_tokenSettings.SecretKey);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = _tokenSettings.ValidateIssuerSigningKey,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = _tokenSettings.ValidateIssuer,
                ValidIssuer = _tokenSettings.Issuer,
                ValidateAudience = _tokenSettings.ValidateAudience,
                ValidAudience = _tokenSettings.Audience,
                ValidateLifetime = true,
                RequireExpirationTime = true,
                RequireSignedTokens = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            var principal = _tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);

            var purposeClaim = principal.FindFirst("purpose")?.Value;
            if (purposeClaim != "password_reset")
            {
                _logger.LogWarning("Token validation failed: not a password reset token");
                return (false, string.Empty, null);
            }

            var userIdClaim = principal.FindFirst("user_id")?.Value ??
                              principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                              principal.FindFirst("sub")?.Value;

            var emailClaim = principal.FindFirst("email")?.Value ??
                             principal.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim) || string.IsNullOrWhiteSpace(emailClaim))
            {
                _logger.LogWarning(
                    "Token validation failed: missing or invalid user claims - UserId: {UserId}, Email: {Email}",
                    userIdClaim, emailClaim);
                return (false, string.Empty, null);
            }

            _logger.LogInformation("Password reset token validation successful for user: {UserId}", userIdClaim);
            return (true, userIdClaim, emailClaim);
        }
        catch (SecurityTokenExpiredException ex)
        {
            _logger.LogWarning("Password reset token validation failed: token expired - {Message}", ex.Message);
            return (false, string.Empty, null);
        }
        catch (SecurityTokenInvalidSignatureException ex)
        {
            _logger.LogWarning("Password reset token validation failed: invalid signature - {Message}", ex.Message);
            return (false, string.Empty, null);
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning("Password reset token validation failed: {Message}", ex.Message);
            return (false, string.Empty, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during password reset token validation");
            return (false, string.Empty, null);
        }
    }
}