namespace SoftFocusBackend.Crisis.Domain.Model.ValueObjects;

public record EmotionalContext
{
    public string? LastDetectedEmotion { get; init; }
    public DateTime? LastEmotionDetectedAt { get; init; }
    public string? EmotionSource { get; init; }

    public EmotionalContext(string? lastDetectedEmotion, DateTime? lastEmotionDetectedAt, string? emotionSource)
    {
        LastDetectedEmotion = lastDetectedEmotion;
        LastEmotionDetectedAt = lastEmotionDetectedAt;
        EmotionSource = emotionSource;
    }

    public static EmotionalContext Empty() => new(null, null, null);

    public bool HasEmotionalData() => !string.IsNullOrWhiteSpace(LastDetectedEmotion);
}
