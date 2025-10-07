using SoftFocusBackend.AI.Domain.Model.ValueObjects;

namespace SoftFocusBackend.AI.Domain.Services;

public interface IEmotionalChatService
{
    Task<ChatResponse> SendMessageAsync(GeminiContext context);
}

public record ChatResponse
{
    public string Reply { get; init; } = string.Empty;
    public List<string> SuggestedQuestions { get; init; } = new();
    public List<string> RecommendedExercises { get; init; } = new();
    public bool CrisisDetected { get; set; }
    public string CrisisContext { get; set; } = string.Empty;
}
