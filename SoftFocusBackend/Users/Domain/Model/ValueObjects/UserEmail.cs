using System.Text.RegularExpressions;

namespace SoftFocusBackend.Users.Domain.Model.ValueObjects;

public partial record UserEmail
{
    public string Value { get; init; }

    public UserEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty.", nameof(email));

        var normalizedEmail = email.Trim().ToLowerInvariant();

        if (!IsValidEmailFormat(normalizedEmail))
            throw new ArgumentException("Invalid email format.", nameof(email));

        if (normalizedEmail.Length > 100)
            throw new ArgumentException("Email cannot exceed 100 characters.", nameof(email));

        Value = normalizedEmail;
    }

    private static bool IsValidEmailFormat(string email)
    {
        return EmailRegex().IsMatch(email);
    }

    [GeneratedRegex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.Compiled)]
    private static partial Regex EmailRegex();

    public static implicit operator string(UserEmail email) => email.Value;
    public static implicit operator UserEmail(string email) => new(email);
}