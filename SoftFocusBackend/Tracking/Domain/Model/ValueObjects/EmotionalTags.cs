namespace SoftFocusBackend.Tracking.Domain.Model.ValueObjects;

public record EmotionalTags
{
    public List<string> Value { get; init; }

    public EmotionalTags(List<string> tags)
    {
        if (tags == null)
            throw new ArgumentException("Emotional tags list cannot be null.", nameof(tags));

        if (tags.Count > 10)
            throw new ArgumentException("Cannot have more than 10 emotional tags.", nameof(tags));

        Value = tags.Where(t => !string.IsNullOrWhiteSpace(t))
                   .Select(t => t.Trim())
                   .ToList();
    }

    public static implicit operator List<string>(EmotionalTags tags) => tags.Value;
    public static implicit operator EmotionalTags(List<string> tags) => new(tags);
}