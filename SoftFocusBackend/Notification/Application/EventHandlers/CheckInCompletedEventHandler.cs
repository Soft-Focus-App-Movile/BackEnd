using SoftFocusBackend.Notification.Application.Internal.CommandServices;
using SoftFocusBackend.Tracking.Domain.Model.Events;

namespace SoftFocusBackend.Notification.Application.EventHandlers;

/// <summary>
/// Handler: Notifica al psicólogo sobre check-ins críticos
/// </summary>
public class CheckInCompletedEventHandler
{
    private readonly SendNotificationCommandService _notificationService;
    private readonly ILogger<CheckInCompletedEventHandler> _logger;

    public CheckInCompletedEventHandler(
        SendNotificationCommandService notificationService,
        ILogger<CheckInCompletedEventHandler> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task HandleAsync(CheckInCompletedEvent evt)
    {
        // Solo notificar si es crítico (niveles bajos)
        if (!evt.IsCritical) return;

        _logger.LogInformation(
            "Handling critical CheckInCompletedEvent: Patient {PatientId} has low levels",
            evt.PatientId);

        try
        {
            // Aquí necesitarías obtener el psychologistId desde el relationship
            // Por ahora, lo dejamos como placeholder
            
            _logger.LogInformation("Critical check-in detected for patient {PatientId}", evt.PatientId);
            // TODO: Implement psychologist notification after getting relationship
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling critical check-in: {Error}", ex.Message);
        }
    }
}