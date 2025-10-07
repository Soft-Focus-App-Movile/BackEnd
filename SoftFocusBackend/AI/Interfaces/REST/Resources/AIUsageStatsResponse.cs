namespace SoftFocusBackend.AI.Interfaces.REST.Resources;

public record AIUsageStatsResponse
{
    public int ChatMessagesUsed { get; init; }
    public int ChatMessagesLimit { get; init; }
    public int FacialAnalysisUsed { get; init; }
    public int FacialAnalysisLimit { get; init; }
    public int RemainingMessages { get; init; }
    public int RemainingAnalyses { get; init; }
    public string CurrentWeek { get; init; } = string.Empty;
    public DateTime ResetsAt { get; init; }
    public string Plan { get; init; } = string.Empty;
}
