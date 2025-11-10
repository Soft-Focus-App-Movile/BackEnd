using SoftFocusBackend.Notification.Application.Internal.CommandServices;
using SoftFocusBackend.Notification.Domain.Model.Commands;
using SoftFocusBackend.Therapy.Domain.Model.Events;


namespace SoftFocusBackend.Notification.Application.EventHandlers;

/// <summary>
/// Handler: Notifica cuando llega un nuevo mensaje de chat
/// </summary>
public class MessageSentEventHandler
{
    private readonly SendNotificationCommandService _notificationService;
    private readonly ILogger<MessageSentEventHandler> _logger;

    public MessageSentEventHandler(
        SendNotificationCommandService notificationService,
        ILogger<MessageSentEventHandler> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task HandleAsync(MessageSentEvent evt)
    {
        _logger.LogInformation(
            "Handling MessageSentEvent: Message {MessageId} from {SenderId} to {ReceiverId}",
            evt.MessageId, evt.SenderId, evt.ReceiverId);

        try
        {
            var senderRole = evt.SenderIsPsychologist ? "tu psicólogo" : "tu paciente";
            
            var command = new SendNotificationCommand(
                UserId: evt.ReceiverId,
                Type: "message-received",
                Title: $"Nuevo mensaje de {senderRole}",
                Content: evt.Content.Length > 100 
                    ? $"{evt.Content.Substring(0, 100)}..." 
                    : evt.Content,
                Priority: "normal",
                DeliveryMethod: "push",
                ScheduledAt: null,
                Metadata: new Dictionary<string, object>
                {
                    { "relationshipId", evt.RelationshipId },
                    { "messageId", evt.MessageId },
                    { "senderId", evt.SenderId }
                }
            );

            await _notificationService.HandleAsync(command);
            
            _logger.LogInformation("Notification sent for message {MessageId}", evt.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error sending notification for message {MessageId}: {Error}", 
                evt.MessageId, ex.Message);
        }
    }
}