using MongoDB.Bson;

namespace SoftFocusBackend.Tracking.Domain.Model.ValueObjects;

public record UserId
{
    public string Value { get; init; }

    public UserId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("User ID cannot be null or empty.", nameof(id));

        if (!ObjectId.TryParse(id, out _))
            throw new ArgumentException("Invalid ObjectId format.", nameof(id));

        Value = id;
    }

    public static implicit operator string(UserId userId) => userId.Value;
    public static implicit operator UserId(string userId) => new(userId);
}