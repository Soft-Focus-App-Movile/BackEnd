using Microsoft.Extensions.Logging;
using SoftFocusBackend.AI.Application.ACL.Services;
using SoftFocusBackend.AI.Domain.Model.ValueObjects;

namespace SoftFocusBackend.AI.Application.ACL.Implementations;

public class TrackingIntegrationService : ITrackingIntegrationService
{
    private readonly ILogger<TrackingIntegrationService> _logger;

    public TrackingIntegrationService(ILogger<TrackingIntegrationService> logger)
    {
        _logger = logger;
    }

    public Task<List<CheckInSummary>> GetRecentCheckInsAsync(string userId, int days)
    {
        _logger.LogWarning("Tracking context not implemented yet. Returning empty check-ins list for user {UserId}", userId);
        return Task.FromResult(new List<CheckInSummary>());
    }

    public Task<string> CreateAutoCheckInAsync(string userId, string emotion, double confidence)
    {
        _logger.LogWarning("Tracking context not implemented yet. Mock check-in created for user {UserId}, emotion: {Emotion}",
            userId, emotion);
        return Task.FromResult(Guid.NewGuid().ToString());
    }

    public Task<string> GetEmotionalPatternAsync(string userId)
    {
        _logger.LogWarning("Tracking context not implemented yet. Returning 'unknown' pattern for user {UserId}", userId);
        return Task.FromResult("unknown");
    }
}
