namespace SoftFocusBackend.AI.Interfaces.REST.Resources;

public record ChatMessageResponse
{
    public string SessionId { get; init; } = string.Empty;
    public string Reply { get; init; } = string.Empty;
    public List<string> SuggestedQuestions { get; init; } = new();
    public List<ExerciseRecommendation> RecommendedExercises { get; init; } = new();
    public bool CrisisDetected { get; init; }
    public DateTime Timestamp { get; init; }
}

public record ExerciseRecommendation
{
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Duration { get; init; } = string.Empty;
}
