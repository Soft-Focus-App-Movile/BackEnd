using Microsoft.Extensions.Logging;
using SoftFocusBackend.AI.Application.ACL.Services;

namespace SoftFocusBackend.AI.Application.ACL.Implementations;

public class TherapyIntegrationService : ITherapyIntegrationService
{
    private readonly ILogger<TherapyIntegrationService> _logger;

    public TherapyIntegrationService(ILogger<TherapyIntegrationService> logger)
    {
        _logger = logger;
    }

    public Task<List<string>> GetCurrentTherapyGoalsAsync(string userId)
    {
        _logger.LogWarning("Therapy context not implemented yet. Returning empty goals list for user {UserId}", userId);
        return Task.FromResult(new List<string>());
    }

    public Task<List<string>> GetAssignedExercisesAsync(string userId)
    {
        _logger.LogWarning("Therapy context not implemented yet. Returning empty exercises list for user {UserId}", userId);
        return Task.FromResult(new List<string>());
    }
}
