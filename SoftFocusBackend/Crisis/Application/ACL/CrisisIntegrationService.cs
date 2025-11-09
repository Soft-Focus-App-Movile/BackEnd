using SoftFocusBackend.Crisis.Application.Internal.CommandServices;
using SoftFocusBackend.Crisis.Domain.Model.Commands;
using SoftFocusBackend.Crisis.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Crisis.Application.ACL;

public class CrisisIntegrationService : ICrisisIntegrationService
{
    private readonly ICrisisAlertCommandService _commandService;

    public CrisisIntegrationService(ICrisisAlertCommandService commandService)
    {
        _commandService = commandService;
    }

    public async Task CreateAlertFromAIChatAsync(
        string patientId,
        string triggerReason,
        AlertSeverity severity)
    {
        var command = new CreateCrisisAlertCommand(
            PatientId: patientId,
            Severity: severity,
            TriggerSource: "AI_CHAT",
            TriggerReason: triggerReason
        );

        await _commandService.Handle(command);
    }

    public async Task CreateAlertFromEmotionAnalysisAsync(
        string patientId,
        string detectedEmotion,
        DateTime emotionDetectedAt)
    {
        var command = new CreateCrisisAlertCommand(
            PatientId: patientId,
            Severity: AlertSeverity.High,
            TriggerSource: "EMOTION_ANALYSIS",
            TriggerReason: "Negative emotions detected repeatedly",
            LastDetectedEmotion: detectedEmotion,
            LastEmotionDetectedAt: emotionDetectedAt,
            EmotionSource: "Facial Analysis"
        );

        await _commandService.Handle(command);
    }

    public async Task CreateAlertFromCheckInAsync(
        string patientId,
        string triggerReason)
    {
        var command = new CreateCrisisAlertCommand(
            PatientId: patientId,
            Severity: AlertSeverity.Moderate,
            TriggerSource: "CHECK_IN",
            TriggerReason: triggerReason
        );

        await _commandService.Handle(command);
    }
}
