using Microsoft.Extensions.Logging;
using SoftFocusBackend.AI.Application.ACL.Services;
using SoftFocusBackend.AI.Domain.Model.ValueObjects;
using SoftFocusBackend.Tracking.Application.Internal.OutboundServices;
using SoftFocusBackend.Tracking.Domain.Services;
using SoftFocusBackend.Tracking.Domain.Model.Commands;

namespace SoftFocusBackend.AI.Application.ACL.Implementations;

/// <summary>
/// Implementación del servicio ACL para integración con Tracking Context
/// Mapea datos del bounded context Tracking al dominio AI
/// </summary>
public class TrackingIntegrationService : ITrackingIntegrationService
{
    private readonly ILogger<TrackingIntegrationService> _logger;
    private readonly ITrackingFacade _trackingFacade;
    private readonly ICheckInCommandService _checkInCommandService;

    public TrackingIntegrationService(
        ILogger<TrackingIntegrationService> logger,
        ITrackingFacade trackingFacade,
        ICheckInCommandService checkInCommandService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _trackingFacade = trackingFacade ?? throw new ArgumentNullException(nameof(trackingFacade));
        _checkInCommandService = checkInCommandService ?? throw new ArgumentNullException(nameof(checkInCommandService));
    }

    public async Task<List<CheckInSummary>> GetRecentCheckInsAsync(string userId, int days)
    {
        try
        {
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-days);

            var checkIns = await _trackingFacade.GetUserCheckInsAsync(userId, startDate, endDate);

            if (checkIns == null || !checkIns.Any())
            {
                _logger.LogDebug("No check-ins found for user {UserId} in the last {Days} days", userId, days);
                return new List<CheckInSummary>();
            }

            // Map CheckIn entities to CheckInSummary DTOs
            var checkInSummaries = checkIns
                .OrderByDescending(c => c.CompletedAt)
                .Select(c => new CheckInSummary(
                    Date: c.CompletedAt.ToString("yyyy-MM-dd"),
                    Emotion: MapEmotionalLevelToEmotion(c.EmotionalLevel),
                    Intensity: c.EmotionalLevel / 10.0, // Normalize to 0-1
                    Note: string.IsNullOrWhiteSpace(c.MoodDescription) ? null : c.MoodDescription
                ))
                .ToList();

            _logger.LogDebug(
                "Retrieved {Count} check-ins for user {UserId} from last {Days} days",
                checkInSummaries.Count,
                userId,
                days);

            return checkInSummaries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent check-ins for user {UserId}", userId);
            return new List<CheckInSummary>();
        }
    }

    public async Task<string> CreateAutoCheckInAsync(string userId, string emotion, double confidence)
    {
        try
        {
            // Validate that user hasn't completed check-in today to avoid conflicts
            if (await _trackingFacade.HasUserCompletedCheckInTodayAsync(userId))
            {
                _logger.LogInformation(
                    "User {UserId} already completed check-in today. Skipping auto check-in creation.",
                    userId);
                return string.Empty; // Return empty to indicate no check-in was created
            }

            // Map emotion string and confidence to EmotionalLevel (1-10)
            var emotionalLevel = MapEmotionToEmotionalLevel(emotion, confidence);

            // Create check-in command with AI-detected emotion
            var command = new CreateCheckInCommand(
                userId: userId,
                emotionalLevel: emotionalLevel,
                energyLevel: 5, // Default mid-level energy
                moodDescription: $"Auto-detected: {emotion} (confidence: {confidence:P0})",
                sleepHours: 0, // No sleep data from AI
                symptoms: new List<string>(),
                notes: "Automatically created by AI emotional analysis"
            );

            var checkIn = await _checkInCommandService.HandleCreateCheckInAsync(command);

            if (checkIn != null)
            {
                _logger.LogInformation(
                    "Auto check-in created successfully for user {UserId}: {CheckInId}, emotion: {Emotion}",
                    userId,
                    checkIn.Id,
                    emotion);
                return checkIn.Id;
            }

            _logger.LogWarning("Failed to create auto check-in for user {UserId}", userId);
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating auto check-in for user {UserId}", userId);
            return string.Empty;
        }
    }

    public async Task<string> GetEmotionalPatternAsync(string userId)
    {
        try
        {
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-30); // Last 30 days

            var checkIns = await _trackingFacade.GetUserCheckInsAsync(userId, startDate, endDate);

            if (checkIns == null || !checkIns.Any())
            {
                _logger.LogDebug("No check-ins found for pattern analysis for user {UserId}", userId);
                return "unknown";
            }

            // Calculate average emotional level
            var avgEmotionalLevel = checkIns.Average(c => c.EmotionalLevel);

            // Get first and last check-in to detect trend
            var orderedCheckIns = checkIns.OrderBy(c => c.CompletedAt).ToList();
            var firstHalf = orderedCheckIns.Take(orderedCheckIns.Count / 2).ToList();
            var secondHalf = orderedCheckIns.Skip(orderedCheckIns.Count / 2).ToList();

            var avgFirstHalf = firstHalf.Any() ? firstHalf.Average(c => c.EmotionalLevel) : 0;
            var avgSecondHalf = secondHalf.Any() ? secondHalf.Average(c => c.EmotionalLevel) : 0;

            // Calculate standard deviation to detect fluctuation
            var variance = checkIns.Average(c => Math.Pow(c.EmotionalLevel - avgEmotionalLevel, 2));
            var stdDev = Math.Sqrt(variance);

            // Determine pattern based on trend and stability
            var pattern = DetermineEmotionalPattern(avgFirstHalf, avgSecondHalf, stdDev);

            _logger.LogDebug(
                "Emotional pattern for user {UserId}: {Pattern} (avg: {Avg:F1}, stdDev: {StdDev:F1})",
                userId,
                pattern,
                avgEmotionalLevel,
                stdDev);

            return pattern;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting emotional pattern for user {UserId}", userId);
            return "unknown";
        }
    }

    /// <summary>
    /// Maps emotional level (1-10) to emotion string
    /// </summary>
    private string MapEmotionalLevelToEmotion(int emotionalLevel)
    {
        return emotionalLevel switch
        {
            >= 1 and <= 3 => "sad",
            >= 4 and <= 5 => "anxious",
            >= 6 and <= 7 => "calm",
            >= 8 and <= 10 => "happy",
            _ => "neutral"
        };
    }

    /// <summary>
    /// Maps emotion string and confidence to emotional level (1-10)
    /// </summary>
    private int MapEmotionToEmotionalLevel(string emotion, double confidence)
    {
        var baseLevel = emotion.ToLowerInvariant() switch
        {
            "happy" or "joy" or "excited" => 9,
            "calm" or "peaceful" or "relaxed" => 7,
            "neutral" => 5,
            "anxious" or "worried" or "stressed" => 4,
            "sad" or "depressed" or "unhappy" => 2,
            "angry" or "frustrated" => 3,
            _ => 5
        };

        // Adjust based on confidence (lower confidence = move towards neutral)
        var adjustment = (int)Math.Round((baseLevel - 5) * confidence);
        return Math.Clamp(5 + adjustment, 1, 10);
    }

    /// <summary>
    /// Determines emotional pattern based on trend analysis
    /// Returns: "declining", "positive", "stable", "fluctuating"
    /// </summary>
    private string DetermineEmotionalPattern(double avgFirstHalf, double avgSecondHalf, double stdDev)
    {
        var trend = avgSecondHalf - avgFirstHalf;

        // High fluctuation (stdDev > 2.5)
        if (stdDev > 2.5)
        {
            return "fluctuating";
        }

        // Significant decline (trend < -1.5)
        if (trend < -1.5)
        {
            return "declining";
        }

        // Positive improvement (trend > 1.5)
        if (trend > 1.5)
        {
            return "positive";
        }

        // Stable (low fluctuation and small trend)
        return "stable";
    }
}
