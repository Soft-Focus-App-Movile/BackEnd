namespace SoftFocusBackend.AI.Domain.Services;

public interface IFacialEmotionService
{
    Task<EmotionAnalysisResult> AnalyzeAsync(byte[] imageBytes);
}

public record EmotionAnalysisResult
{
    public string PrimaryEmotion { get; init; } = string.Empty;
    public double Confidence { get; init; }
    public Dictionary<string, double> AllEmotions { get; init; } = new();
}
