namespace SoftFocusBackend.AI.Interfaces.REST.Resources;

public record ChatHistoryResponse
{
    public List<ChatSessionResponse> Sessions { get; init; } = new();
    public int TotalCount { get; init; }
}
