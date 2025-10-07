using SoftFocusBackend.AI.Domain.Model.ValueObjects;

namespace SoftFocusBackend.AI.Application.ACL.Services;

public interface ITrackingIntegrationService
{
    Task<List<CheckInSummary>> GetRecentCheckInsAsync(string userId, int days);
    Task<string> CreateAutoCheckInAsync(string userId, string emotion, double confidence);
    Task<string> GetEmotionalPatternAsync(string userId);
}
