using SoftFocusBackend.Notification.Application.Internal.CommandServices;
using SoftFocusBackend.Notification.Domain.Model.Commands;
using SoftFocusBackend.Crisis.Domain.Model.Events;

namespace SoftFocusBackend.Notification.Application.EventHandlers;

/// <summary>
/// Handler: Notifica alertas de crisis CRÍTICAS
/// </summary>
public class CrisisAlertCreatedEventHandler
{
    private readonly SendNotificationCommandService _notificationService;
    private readonly ILogger<CrisisAlertCreatedEventHandler> _logger;

    public CrisisAlertCreatedEventHandler(
        SendNotificationCommandService notificationService,
        ILogger<CrisisAlertCreatedEventHandler> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task HandleAsync(CrisisAlertCreatedEvent evt)
    {
        _logger.LogInformation(
            "Handling CrisisAlertCreatedEvent: Alert {AlertId} for patient {PatientId}",
            evt.AlertId, evt.PatientId);

        try
        {
            var command = new SendNotificationCommand(
                UserId: evt.PsychologistId,
                Type: "crisis-alert",
                Title: "⚠️ ALERTA DE CRISIS",
                Content: $"Alerta {evt.Severity}: {evt.TriggerReason}",
                Priority: "critical",
                DeliveryMethod: "push",
                ScheduledAt: null,
                Metadata: new Dictionary<string, object>
                {
                    { "alertId", evt.AlertId },
                    { "patientId", evt.PatientId },
                    { "severity", evt.Severity },
                    { "triggerSource", evt.TriggerSource }
                }
            );

            await _notificationService.HandleAsync(command);
            
            _logger.LogInformation(
                "Crisis alert notification sent to psychologist {PsychologistId}",
                evt.PsychologistId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending crisis alert notification: {Error}", ex.Message);
        }
    }
}