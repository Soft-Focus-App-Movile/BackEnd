using SoftFocusBackend.Therapy.Domain.Model.Aggregates;
using SoftFocusBackend.Therapy.Domain.Model.Queries;
using SoftFocusBackend.Therapy.Domain.Repositories;

namespace SoftFocusBackend.Therapy.Application.Internal.QueryServices
{
    public class ChatHistoryQueryService
    {
        private readonly IChatMessageRepository _messageRepository;

        public ChatHistoryQueryService(IChatMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

        public async Task<IEnumerable<ChatMessage>> Handle(GetChatHistoryQuery query)
        {
            return await _messageRepository.GetByRelationshipIdAsync(query.RelationshipId, query.Page, query.Size);
        }
        
        public async Task<ChatMessage?> Handle(GetLastMessageQuery query)
        {
            return await _messageRepository.GetLastMessageByReceiverIdAsync(query.ReceiverId);
        }
    }
}