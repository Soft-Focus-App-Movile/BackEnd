using SoftFocusBackend.AI.Domain.Model.ValueObjects;

namespace SoftFocusBackend.AI.Domain.Services;

public interface IGeminiContextBuilder
{
    Task<GeminiContext> BuildContextAsync(string userId, string message, string? sessionId);
}
