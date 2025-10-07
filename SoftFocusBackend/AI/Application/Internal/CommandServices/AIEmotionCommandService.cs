using Microsoft.Extensions.Logging;
using SoftFocusBackend.AI.Application.ACL.Services;
using SoftFocusBackend.AI.Domain.Model.Aggregates;
using SoftFocusBackend.AI.Domain.Model.Commands;
using SoftFocusBackend.AI.Domain.Repositories;
using SoftFocusBackend.AI.Domain.Services;
using SoftFocusBackend.Shared.Infrastructure.ExternalServices.Cloudinary.Services;

namespace SoftFocusBackend.AI.Application.Internal.CommandServices;

public class AIEmotionCommandService
{
    private readonly IAIUsageTracker _usageTracker;
    private readonly IFacialEmotionService _emotionService;
    private readonly ICrisisPatternDetector _crisisDetector;
    private readonly ICrisisIntegrationService _crisisIntegration;
    private readonly ITrackingIntegrationService _trackingIntegration;
    private readonly IEmotionAnalysisRepository _analysisRepository;
    private readonly ICloudinaryImageService _cloudinaryService;
    private readonly ILogger<AIEmotionCommandService> _logger;

    public AIEmotionCommandService(
        IAIUsageTracker usageTracker,
        IFacialEmotionService emotionService,
        ICrisisPatternDetector crisisDetector,
        ICrisisIntegrationService crisisIntegration,
        ITrackingIntegrationService trackingIntegration,
        IEmotionAnalysisRepository analysisRepository,
        ICloudinaryImageService cloudinaryService,
        ILogger<AIEmotionCommandService> logger)
    {
        _usageTracker = usageTracker;
        _emotionService = emotionService;
        _crisisDetector = crisisDetector;
        _crisisIntegration = crisisIntegration;
        _trackingIntegration = trackingIntegration;
        _analysisRepository = analysisRepository;
        _cloudinaryService = cloudinaryService;
        _logger = logger;
    }

    public async Task<EmotionCommandResult> HandleAnalyzeEmotionAsync(AnalyzeFacialEmotionCommand command)
    {
        try
        {
            _logger.LogInformation(command.GetAuditString());

            var canUse = await _usageTracker.CanUseFacialAnalysisAsync(command.UserId);
            if (!canUse)
            {
                _logger.LogWarning("User {UserId} exceeded facial analysis usage limit", command.UserId);
                var usage = await _usageTracker.GetCurrentUsageAsync(command.UserId);
                return EmotionCommandResult.LimitExceeded(usage);
            }

            var fileName = $"{command.UserId}_{DateTime.UtcNow:yyyyMMddHHmmss}.jpg";
            var imageUrl = await _cloudinaryService.UploadImageAsync(command.ImageBytes, fileName, "emotion_analyses");

            var emotionResult = await _emotionService.AnalyzeAsync(command.ImageBytes);

            var analysis = EmotionAnalysis.Create(
                command.UserId,
                emotionResult.PrimaryEmotion,
                emotionResult.Confidence,
                emotionResult.AllEmotions,
                imageUrl
            );

            await _analysisRepository.SaveAsync(analysis);

            var crisisPattern = await _crisisDetector.DetectFromEmotionPatternAsync(command.UserId, analysis);
            if (crisisPattern != null)
            {
                await _crisisIntegration.TriggerCrisisAlertAsync(new CrisisAlertRequest
                {
                    UserId = command.UserId,
                    Source = "facial_pattern",
                    Severity = crisisPattern.GetSeverityString(),
                    TriggerReason = crisisPattern.TriggerReason,
                    Context = $"Detected emotion: {emotionResult.PrimaryEmotion} with confidence {emotionResult.Confidence:P0}",
                    DetectedAt = DateTime.UtcNow
                });
            }

            string? checkInId = null;
            if (command.AutoCheckIn)
            {
                checkInId = await _trackingIntegration.CreateAutoCheckInAsync(
                    command.UserId,
                    emotionResult.PrimaryEmotion,
                    emotionResult.Confidence
                );

                analysis.MarkCheckInCreated(checkInId);
                await _analysisRepository.UpdateAsync(analysis);
            }

            await _usageTracker.IncrementFacialUsageAsync(command.UserId);

            return EmotionCommandResult.Success(analysis, checkInId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling analyze emotion command for user {UserId}", command.UserId);
            throw;
        }
    }
}

public record EmotionCommandResult
{
    public bool IsSuccess { get; init; }
    public bool IsLimitExceeded { get; init; }
    public EmotionAnalysis? Analysis { get; init; }
    public string? CheckInId { get; init; }
    public AIUsage? Usage { get; init; }

    public static EmotionCommandResult Success(EmotionAnalysis analysis, string? checkInId)
    {
        return new EmotionCommandResult
        {
            IsSuccess = true,
            IsLimitExceeded = false,
            Analysis = analysis,
            CheckInId = checkInId
        };
    }

    public static EmotionCommandResult LimitExceeded(AIUsage usage)
    {
        return new EmotionCommandResult
        {
            IsSuccess = false,
            IsLimitExceeded = true,
            Usage = usage
        };
    }
}
