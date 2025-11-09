using Microsoft.AspNetCore.SignalR;
using SoftFocusBackend.Crisis.Domain.Model.Aggregates;
using SoftFocusBackend.Crisis.Domain.Services;
using SoftFocusBackend.Crisis.Interfaces.Hubs;

namespace SoftFocusBackend.Crisis.Infrastructure.Services;

public class CrisisNotificationService : ICrisisNotificationService
{
    private readonly IHubContext<CrisisHub> _hubContext;

    public CrisisNotificationService(IHubContext<CrisisHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyPsychologistAsync(CrisisAlert alert)
    {
        var notification = new
        {
            alertId = alert.Id,
            patientId = alert.PatientId,
            severity = alert.Severity.ToString(),
            triggerSource = alert.TriggerSource,
            triggerReason = alert.TriggerReason,
            location = alert.Location?.ToDisplayString(),
            emotionalContext = alert.EmotionalContext?.LastDetectedEmotion,
            createdAt = alert.CreatedAt
        };

        await _hubContext.Clients
            .Group($"psychologist-{alert.PsychologistId}")
            .SendAsync("ReceiveCrisisAlert", notification);
    }
}
