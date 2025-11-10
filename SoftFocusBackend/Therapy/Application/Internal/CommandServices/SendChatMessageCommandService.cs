using SoftFocusBackend.Therapy.Domain.Model.Aggregates;
using SoftFocusBackend.Therapy.Domain.Model.Commands;
using SoftFocusBackend.Therapy.Domain.Model.ValueObjects;
using SoftFocusBackend.Therapy.Domain.Model.Events;
using SoftFocusBackend.Therapy.Domain.Repositories;
using SoftFocusBackend.Therapy.Domain.Services;
using SoftFocusBackend.Shared.Infrastructure.Events;

namespace SoftFocusBackend.Therapy.Application.Internal.CommandServices
{
    public class SendChatMessageCommandService
    {
        private readonly IChatMessageRepository _messageRepository;
        private readonly IChatModerationService _moderationService;
        private readonly ITherapeuticRelationshipRepository _relationshipRepository;
        private readonly IDomainEventBus _eventBus; // ← NUEVO
        private readonly ILogger<SendChatMessageCommandService> _logger; // ← NUEVO

        public SendChatMessageCommandService(
            IChatMessageRepository messageRepository,
            IChatModerationService moderationService,
            ITherapeuticRelationshipRepository relationshipRepository,
            IDomainEventBus eventBus,
            ILogger<SendChatMessageCommandService> logger)
        {
            _messageRepository = messageRepository;
            _moderationService = moderationService;
            _relationshipRepository = relationshipRepository;
            _eventBus = eventBus;
            _logger = logger;
        }

        public async Task<ChatMessage> Handle(SendChatMessageCommand command)
        {
            // Validar relación terapéutica
            var relationship = await _relationshipRepository.GetByIdAsync(command.RelationshipId);
            if (relationship == null || !relationship.IsActive)
            {
                _logger.LogWarning(
                    "Invalid or inactive relationship: {RelationshipId}", 
                    command.RelationshipId);
                throw new InvalidOperationException("Invalid or inactive relationship.");
            }

            // Moderar contenido
            var content = MessageContent.Create(command.Content);
            var moderatedContent = await _moderationService.ModerateContentAsync(content);

            // Crear mensaje
            var message = new ChatMessage(
                command.RelationshipId,
                command.SenderId,
                command.ReceiverId,
                moderatedContent,
                command.MessageType);

            await _messageRepository.AddAsync(message);

            // 🔥 NUEVO: Publicar evento de dominio
            try
            {
                // Determinar si el sender es psicólogo
                var senderIsPsychologist = relationship.PsychologistId == command.SenderId;

                var messageEvent = new MessageSentEvent(
                    messageId: message.Id,
                    relationshipId: command.RelationshipId,
                    senderId: command.SenderId,
                    receiverId: command.ReceiverId,
                    content: moderatedContent.Value, // Asumiendo que MessageContent tiene una propiedad Value
                    messageType: command.MessageType,
                    senderIsPsychologist: senderIsPsychologist
                );

                await _eventBus.PublishAsync(messageEvent);

                _logger.LogInformation(
                    "MessageSentEvent published for message {MessageId}", 
                    message.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error publishing MessageSentEvent for message {MessageId}: {Error}", 
                    message.Id, ex.Message);
                // No lanzamos la excepción para no afectar el flujo principal
            }

            return message;
        }
    }
}