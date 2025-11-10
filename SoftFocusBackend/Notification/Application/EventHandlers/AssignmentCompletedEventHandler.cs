using SoftFocusBackend.Notification.Application.Internal.CommandServices;
using SoftFocusBackend.Notification.Domain.Model.Commands;
using SoftFocusBackend.Library.Domain.Model.Events;


namespace SoftFocusBackend.Notification.Application.EventHandlers;

/// <summary>
/// Handler: Notifica al psicólogo cuando el paciente completa una tarea
/// </summary>
public class AssignmentCompletedEventHandler
{
    private readonly SendNotificationCommandService _notificationService;
    private readonly ILogger<AssignmentCompletedEventHandler> _logger;

    public AssignmentCompletedEventHandler(
        SendNotificationCommandService notificationService,
        ILogger<AssignmentCompletedEventHandler> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task HandleAsync(AssignmentCompletedEvent evt)
    {
        _logger.LogInformation(
            "Handling AssignmentCompletedEvent: Assignment {AssignmentId} completed by patient {PatientId}",
            evt.AssignmentId, evt.PatientId);

        try
        {
            var command = new SendNotificationCommand(
                UserId: evt.PsychologistId,
                Type: "assignment-completed",
                Title: "Tarea completada",
                Content: $"Tu paciente completó la tarea: {evt.ContentTitle}",
                Priority: "normal",
                DeliveryMethod: "push",
                ScheduledAt: null,
                Metadata: new Dictionary<string, object>
                {
                    { "assignmentId", evt.AssignmentId },
                    { "patientId", evt.PatientId },
                    { "contentType", evt.ContentType }
                }
            );

            await _notificationService.HandleAsync(command);
            
            _logger.LogInformation(
                "Notification sent to psychologist {PsychologistId} for completed assignment",
                evt.PsychologistId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error sending completion notification: {Error}", ex.Message);
        }
    }
}