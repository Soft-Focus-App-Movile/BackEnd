namespace SoftFocusBackend.Auth.Infrastructure.Tokens.JWT.Configuration;

public class TokenSettings
{
    public const string SectionName = "TokenSettings";

    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationHours { get; set; }
    public int ClockSkewMinutes { get; set; }
    public bool ValidateIssuer { get; set; }
    public bool ValidateAudience { get; set; }
    public bool ValidateLifetime { get; set; }
    public bool ValidateIssuerSigningKey { get; set; }
    public bool RequireExpirationTime { get; set; }
    public bool RequireSignedTokens { get; set; }

    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(SecretKey) &&
               SecretKey.Length >= 32 &&
               !string.IsNullOrWhiteSpace(Issuer) &&
               !string.IsNullOrWhiteSpace(Audience) &&
               ExpirationHours > 0 &&
               ClockSkewMinutes >= 0;
    }

    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(SecretKey))
            errors.Add("SecretKey is required");
        else if (SecretKey.Length < 32)
            errors.Add("SecretKey must be at least 32 characters long");

        if (string.IsNullOrWhiteSpace(Issuer))
            errors.Add("Issuer is required");

        if (string.IsNullOrWhiteSpace(Audience))
            errors.Add("Audience is required");

        if (ExpirationHours <= 0)
            errors.Add("ExpirationHours must be greater than 0");

        if (ClockSkewMinutes < 0)
            errors.Add("ClockSkewMinutes cannot be negative");

        return errors;
    }
}