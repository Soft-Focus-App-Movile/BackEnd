using SoftFocusBackend.Crisis.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Crisis.Application.ACL;

public interface ICrisisIntegrationService
{
    Task CreateAlertFromAIChatAsync(
        string patientId,
        string triggerReason,
        AlertSeverity severity);

    Task CreateAlertFromEmotionAnalysisAsync(
        string patientId,
        string detectedEmotion,
        DateTime emotionDetectedAt);

    Task CreateAlertFromCheckInAsync(
        string patientId,
        string triggerReason);
}
