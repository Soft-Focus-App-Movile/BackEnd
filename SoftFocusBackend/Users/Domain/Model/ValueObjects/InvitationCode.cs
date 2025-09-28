namespace SoftFocusBackend.Users.Domain.Model.ValueObjects;

public record InvitationCode
{
    public string Value { get; init; }
    public DateTime GeneratedAt { get; init; }
    public DateTime ExpiresAt { get; init; }

    public InvitationCode(string value, DateTime generatedAt, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Invitation code cannot be null or empty.", nameof(value));

        if (value.Length != 8)
            throw new ArgumentException("Invitation code must be exactly 8 characters.", nameof(value));

        if (!IsValidCodeFormat(value))
            throw new ArgumentException("Invitation code must contain only alphanumeric characters.", nameof(value));

        if (expiresAt <= generatedAt)
            throw new ArgumentException("Expiration date must be after generation date.", nameof(expiresAt));

        Value = value.ToUpperInvariant();
        GeneratedAt = generatedAt;
        ExpiresAt = expiresAt;
    }

    public static InvitationCode Generate()
    {
        var now = DateTime.UtcNow;
        var expires = now.AddDays(1);
        var code = GenerateRandomCode();
        return new InvitationCode(code, now, expires);
    }

    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;
    
    public bool IsValid() => !IsExpired() && !string.IsNullOrWhiteSpace(Value);

    public TimeSpan TimeUntilExpiration() => ExpiresAt - DateTime.UtcNow;

    private static bool IsValidCodeFormat(string code)
    {
        return code.All(c => char.IsLetterOrDigit(c));
    }

    private static string GenerateRandomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public static implicit operator string(InvitationCode code) => code.Value;
}