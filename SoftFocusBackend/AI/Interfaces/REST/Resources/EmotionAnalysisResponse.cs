namespace SoftFocusBackend.AI.Interfaces.REST.Resources;

public record EmotionAnalysisResponse
{
    public string AnalysisId { get; init; } = string.Empty;
    public string Emotion { get; init; } = string.Empty;
    public double Confidence { get; init; }
    public Dictionary<string, double> AllEmotions { get; init; } = new();
    public DateTime AnalyzedAt { get; init; }
    public bool CheckInCreated { get; init; }
    public string? CheckInId { get; init; }
}
