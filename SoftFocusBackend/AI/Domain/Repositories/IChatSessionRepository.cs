using SoftFocusBackend.AI.Domain.Model.Aggregates;

namespace SoftFocusBackend.AI.Domain.Repositories;

public interface IChatSessionRepository
{
    Task<ChatSession?> GetByIdAsync(string sessionId);
    Task<ChatSession> CreateAsync(string userId);
    Task AddMessageAsync(string sessionId, ChatMessage message);
    Task<List<ChatSession>> GetUserSessionsAsync(string userId, DateTime? from, DateTime? to, int pageSize);
    Task<List<ChatMessage>> GetSessionMessagesAsync(string sessionId, int limit = 10);
    Task UpdateAsync(ChatSession session);
}
