using SoftFocusBackend.Therapy.Domain.Model.Aggregates;
using SoftFocusBackend.Therapy.Domain.Model.Commands;
using SoftFocusBackend.Therapy.Domain.Model.ValueObjects;
using SoftFocusBackend.Therapy.Domain.Repositories;
using SoftFocusBackend.Therapy.Domain.Services;

namespace SoftFocusBackend.Therapy.Application.Internal.CommandServices
{
    public class SendChatMessageCommandService
    {
        private readonly IChatMessageRepository _messageRepository;
        private readonly IChatModerationService _moderationService;
        private readonly ITherapeuticRelationshipRepository _relationshipRepository;

        public SendChatMessageCommandService(
            IChatMessageRepository messageRepository,
            IChatModerationService moderationService,
            ITherapeuticRelationshipRepository relationshipRepository)
        {
            _messageRepository = messageRepository;
            _moderationService = moderationService;
            _relationshipRepository = relationshipRepository;
        }

        public async Task<ChatMessage> Handle(SendChatMessageCommand command)
        {
            var relationship = await _relationshipRepository.GetByIdAsync(command.RelationshipId);
            if (relationship == null || !relationship.IsActive)
                throw new InvalidOperationException("Invalid or inactive relationship.");

            var content = MessageContent.Create(command.Content);
            var moderatedContent = await _moderationService.ModerateContentAsync(content);

            var message = new ChatMessage(
                command.RelationshipId,
                command.SenderId,
                command.ReceiverId,
                moderatedContent,
                command.MessageType);

            await _messageRepository.AddAsync(message);

            return message;
        }
    }
}