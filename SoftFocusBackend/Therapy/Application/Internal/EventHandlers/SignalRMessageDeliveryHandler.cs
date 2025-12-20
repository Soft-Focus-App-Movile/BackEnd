using SoftFocusBackend.Therapy.Domain.Model.Events;
using SoftFocusBackend.Therapy.Infrastructure.ExternalServices;

namespace SoftFocusBackend.Therapy.Application.Internal.EventHandlers
{
    /// <summary>
    /// Handler: Entrega mensajes en tiempo real vía SignalR
    /// </summary>
    public class SignalRMessageDeliveryHandler
    {
        private readonly SignalRChatService _signalRService;
        private readonly ILogger<SignalRMessageDeliveryHandler> _logger;

        public SignalRMessageDeliveryHandler(
            SignalRChatService signalRService,
            ILogger<SignalRMessageDeliveryHandler> logger)
        {
            _signalRService = signalRService;
            _logger = logger;
        }

        public async Task HandleAsync(MessageSentEvent evt)
        {
            _logger.LogInformation(
                "📨 Entregando mensaje {MessageId} vía SignalR a {ReceiverId}",
                evt.MessageId, evt.ReceiverId);

            try
            {
                // Este objeto DEBE coincidir con TherapyChatResponseDto del frontend
                var messageDto = new
                {
                    id = evt.MessageId,
                    relationshipId = evt.RelationshipId,
                    senderId = evt.SenderId,
                    receiverId = evt.ReceiverId,
                    content = new { value = evt.Content }, // ← Estructura anidada crítica
                    timestamp = DateTime.UtcNow.ToString("o"), // ISO 8601
                    isRead = false,
                    messageType = evt.MessageType
                };

                await _signalRService.SendMessageAsync(evt.ReceiverId, messageDto);

                _logger.LogInformation(
                    "✅ Mensaje {MessageId} entregado exitosamente vía SignalR",
                    evt.MessageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ Error entregando mensaje {MessageId} vía SignalR: {Error}",
                    evt.MessageId, ex.Message);
                // No lanzamos la excepción para no afectar otros handlers
            }
        }
    }
}