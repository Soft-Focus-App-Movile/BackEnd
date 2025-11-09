using Microsoft.Extensions.Logging;
using SoftFocusBackend.Library.Application.ACL.Services;
using SoftFocusBackend.Library.Domain.Model.ValueObjects;
using SoftFocusBackend.Tracking.Application.Internal.OutboundServices;

namespace SoftFocusBackend.Library.Application.ACL.Implementations;

/// <summary>
/// Implementación del servicio ACL para integración con Tracking Context
/// Mapea datos del bounded context Tracking al dominio Library
/// </summary>
public class TrackingIntegrationService : ITrackingIntegrationService
{
    private readonly ILogger<TrackingIntegrationService> _logger;
    private readonly ITrackingFacade _trackingFacade;

    public TrackingIntegrationService(
        ILogger<TrackingIntegrationService> logger,
        ITrackingFacade trackingFacade)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _trackingFacade = trackingFacade ?? throw new ArgumentNullException(nameof(trackingFacade));
    }

    public async Task<EmotionalTag?> GetCurrentEmotionAsync(string userId)
    {
        try
        {
            var todayCheckIn = await _trackingFacade.GetUserTodayCheckInAsync(userId);

            if (todayCheckIn == null)
            {
                _logger.LogDebug("No check-in found for user today: {UserId}", userId);
                return null;
            }

            // Mapear EmotionalLevel (1-10) a EmotionalTag enum
            var emotionalTag = MapEmotionalLevelToTag(todayCheckIn.EmotionalLevel, todayCheckIn.MoodDescription);

            _logger.LogDebug(
                "Mapped emotion for user {UserId}: Level {Level} → {Tag}",
                userId,
                todayCheckIn.EmotionalLevel,
                emotionalTag);

            return emotionalTag;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current emotion for user: {UserId}", userId);
            return null;
        }
    }

    public Task<bool> IsTrackingContextAvailableAsync()
    {
        // Tracking context is now available
        return Task.FromResult(true);
    }

    public async Task<List<EmotionHistory>> GetRecentEmotionHistoryAsync(string userId, int limit = 10)
    {
        try
        {
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-30); // Last 30 days

            var checkIns = await _trackingFacade.GetUserCheckInsAsync(userId, startDate, endDate);

            if (checkIns == null || !checkIns.Any())
            {
                _logger.LogDebug("No check-in history found for user: {UserId}", userId);
                return new List<EmotionHistory>();
            }

            // Take only the requested limit and map to EmotionHistory
            var emotionHistory = checkIns
                .OrderByDescending(c => c.CompletedAt)
                .Take(limit)
                .Select(checkIn => new EmotionHistory
                {
                    Emotion = MapEmotionalLevelToTag(checkIn.EmotionalLevel, checkIn.MoodDescription),
                    RecordedAt = checkIn.CompletedAt,
                    Intensity = checkIn.EmotionalLevel / 10.0 // Normalize to 0-1
                })
                .ToList();

            _logger.LogDebug(
                "Retrieved {Count} emotion history entries for user: {UserId}",
                emotionHistory.Count,
                userId);

            return emotionHistory;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting emotion history for user: {UserId}", userId);
            return new List<EmotionHistory>();
        }
    }

    /// <summary>
    /// Mapea el nivel emocional (1-10) y descripción de humor a EmotionalTag
    /// Mapeo:
    /// - 1-3: Sad
    /// - 4-5 + keywords "anxious": Anxious
    /// - 6-7: Calm
    /// - 8-10: Happy
    /// - Keywords "energetic": Energetic
    /// </summary>
    private EmotionalTag MapEmotionalLevelToTag(int emotionalLevel, string moodDescription)
    {
        var moodLower = moodDescription?.ToLowerInvariant() ?? string.Empty;

        // Check for energetic keywords first
        if (moodLower.Contains("energetic") || moodLower.Contains("motivated") ||
            moodLower.Contains("active") || moodLower.Contains("energía"))
        {
            return EmotionalTag.Energetic;
        }

        // Check for anxious keywords in mid-range
        if ((emotionalLevel >= 4 && emotionalLevel <= 5) &&
            (moodLower.Contains("anxious") || moodLower.Contains("stressed") ||
             moodLower.Contains("worried") || moodLower.Contains("ansioso") ||
             moodLower.Contains("estresado") || moodLower.Contains("preocupado")))
        {
            return EmotionalTag.Anxious;
        }

        // Map by emotional level
        return emotionalLevel switch
        {
            >= 1 and <= 3 => EmotionalTag.Sad,
            >= 4 and <= 5 => EmotionalTag.Anxious, // Default for mid-low range
            >= 6 and <= 7 => EmotionalTag.Calm,
            >= 8 and <= 10 => EmotionalTag.Happy,
            _ => EmotionalTag.Calm // Default fallback
        };
    }
}
