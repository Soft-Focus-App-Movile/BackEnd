using SoftFocusBackend.Therapy.Domain.Model.Aggregates;

namespace SoftFocusBackend.Therapy.Domain.Repositories
{
    public interface IChatMessageRepository
    {
        Task<ChatMessage?> GetByIdAsync(string id);
        Task<IEnumerable<ChatMessage>> GetByRelationshipIdAsync(string relationshipId, int page, int size);
        Task<IEnumerable<ChatMessage>> GetUnreadByReceiverIdAsync(string receiverId);
        Task AddAsync(ChatMessage message);
        Task UpdateAsync(ChatMessage message);
    }
}