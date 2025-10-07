using SoftFocusBackend.AI.Domain.Model.Aggregates;

namespace SoftFocusBackend.AI.Domain.Model.ValueObjects;

public record GeminiContext
{
    public string UserId { get; init; }
    public string CurrentMessage { get; init; }
    public List<ChatMessage> ConversationHistory { get; init; }
    public List<CheckInSummary> RecentCheckIns { get; init; }
    public List<string> TherapyGoals { get; init; }
    public string EmotionalPattern { get; init; }

    public GeminiContext(string userId, string currentMessage)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required", nameof(userId));

        if (string.IsNullOrWhiteSpace(currentMessage))
            throw new ArgumentException("CurrentMessage is required", nameof(currentMessage));

        UserId = userId;
        CurrentMessage = currentMessage;
        ConversationHistory = new List<ChatMessage>();
        RecentCheckIns = new List<CheckInSummary>();
        TherapyGoals = new List<string>();
        EmotionalPattern = "unknown";
    }

    public GeminiContext WithHistory(List<ChatMessage> history)
    {
        return this with { ConversationHistory = history.TakeLast(10).ToList() };
    }

    public GeminiContext WithCheckIns(List<CheckInSummary> checkIns)
    {
        return this with { RecentCheckIns = checkIns };
    }

    public GeminiContext WithTherapyGoals(List<string> goals)
    {
        return this with { TherapyGoals = goals };
    }

    public GeminiContext WithEmotionalPattern(string pattern)
    {
        return this with { EmotionalPattern = pattern };
    }

    public bool HasHistory() => ConversationHistory.Any();
    public bool HasRecentCheckIns() => RecentCheckIns.Any();
    public bool HasTherapyGoals() => TherapyGoals.Any();
}

public record CheckInSummary(string Date, string Emotion, double Intensity, string? Note);
