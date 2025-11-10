using SoftFocusBackend.Notification.Application.Internal.CommandServices;
using SoftFocusBackend.Notification.Domain.Model.Commands;
using SoftFocusBackend.Library.Domain.Model.Events;


namespace SoftFocusBackend.Notification.Application.EventHandlers;

/// <summary>
/// Handler: Notifica cuando se completan TODAS las tareas
/// </summary>
public class AllAssignmentsCompletedEventHandler
{
    private readonly SendNotificationCommandService _notificationService;
    private readonly ILogger<AllAssignmentsCompletedEventHandler> _logger;

    public AllAssignmentsCompletedEventHandler(
        SendNotificationCommandService notificationService,
        ILogger<AllAssignmentsCompletedEventHandler> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task HandleAsync(AllAssignmentsCompletedEvent evt)
    {
        _logger.LogInformation(
            "Handling AllAssignmentsCompletedEvent: Patient {PatientId} completed all {Count} assignments",
            evt.PatientId, evt.CompletedCount);

        try
        {
            var command = new SendNotificationCommand(
                UserId: evt.PsychologistId,
                Type: "all-assignments-completed",
                Title: "¡Todas las tareas completadas!",
                Content: $"Tu paciente completó todas las {evt.CompletedCount} tareas asignadas. ¡Excelente progreso!",
                Priority: "high",
                DeliveryMethod: "push",
                ScheduledAt: null,
                Metadata: new Dictionary<string, object>
                {
                    { "patientId", evt.PatientId },
                    { "completedCount", evt.CompletedCount }
                }
            );

            await _notificationService.HandleAsync(command);
            
            _logger.LogInformation(
                "All-assignments notification sent to psychologist {PsychologistId}",
                evt.PsychologistId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error sending all-assignments notification: {Error}", ex.Message);
        }
    }
}