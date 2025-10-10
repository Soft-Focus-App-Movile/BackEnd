namespace SoftFocusBackend.AI.Domain.Model.ValueObjects;

public record DetectedEmotion
{
    public EmotionType Type { get; init; }
    public double Confidence { get; init; }

    public DetectedEmotion(EmotionType type, double confidence)
    {
        if (confidence < 0 || confidence > 1)
            throw new ArgumentException("Confidence must be between 0 and 1", nameof(confidence));

        Type = type;
        Confidence = confidence;
    }

    public string GetSpanishName() => Type switch
    {
        EmotionType.Joy => "Alegría",
        EmotionType.Sadness => "Tristeza",
        EmotionType.Anger => "Enojo",
        EmotionType.Fear => "Miedo",
        EmotionType.Surprise => "Sorpresa",
        EmotionType.Disgust => "Disgusto",
        EmotionType.Neutral => "Neutral",
        _ => "Desconocido"
    };

    public string GetEmoji() => Type switch
    {
        EmotionType.Joy => "😊",
        EmotionType.Sadness => "😢",
        EmotionType.Anger => "😠",
        EmotionType.Fear => "😨",
        EmotionType.Surprise => "😲",
        EmotionType.Disgust => "🤢",
        EmotionType.Neutral => "😐",
        _ => "❓"
    };

    public bool IsNegative() => Type is EmotionType.Sadness or EmotionType.Fear or EmotionType.Anger;
    public bool IsPositive() => Type is EmotionType.Joy;
}

public enum EmotionType
{
    Joy,
    Sadness,
    Anger,
    Fear,
    Surprise,
    Disgust,
    Neutral
}
