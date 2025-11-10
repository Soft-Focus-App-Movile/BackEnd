using SoftFocusBackend.Notification.Application.Internal.CommandServices;
using SoftFocusBackend.Notification.Domain.Model.Commands;
using SoftFocusBackend.Library.Domain.Model.Events;

namespace SoftFocusBackend.Notification.Application.EventHandlers;

/// <summary>
/// Handler: Notifica cuando se asigna contenido
/// </summary>
public class ContentAssignedEventHandler
{
    private readonly SendNotificationCommandService _notificationService;
    private readonly ILogger<ContentAssignedEventHandler> _logger;

    public ContentAssignedEventHandler(
        SendNotificationCommandService notificationService,
        ILogger<ContentAssignedEventHandler> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task HandleAsync(ContentAssignedEvent evt)
    {
        _logger.LogInformation(
            "Handling ContentAssignedEvent: Assignment {AssignmentId} for patient {PatientId}",
            evt.AssignmentId, evt.PatientId);

        try
        {
            var command = new SendNotificationCommand(
                UserId: evt.PatientId,
                Type: "assignment-received",
                Title: "Nueva tarea asignada",
                Content: string.IsNullOrEmpty(evt.Notes)
                    ? $"Tu psicólogo te asignó: {evt.ContentTitle}"
                    : $"Tu psicólogo te asignó: {evt.ContentTitle}\n\n{evt.Notes}",
                Priority: "normal",
                DeliveryMethod: "push",
                ScheduledAt: null,
                Metadata: new Dictionary<string, object>
                {
                    { "assignmentId", evt.AssignmentId },
                    { "contentId", evt.ContentId },
                    { "contentType", evt.ContentType },
                    { "psychologistId", evt.PsychologistId }
                }
            );

            await _notificationService.HandleAsync(command);
            
            _logger.LogInformation("Notification sent for assignment {AssignmentId}", evt.AssignmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error sending notification for assignment {AssignmentId}: {Error}", 
                evt.AssignmentId, ex.Message);
        }
    }
}