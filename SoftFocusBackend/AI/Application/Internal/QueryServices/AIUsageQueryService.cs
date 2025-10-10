using Microsoft.Extensions.Logging;
using SoftFocusBackend.AI.Domain.Model.Aggregates;
using SoftFocusBackend.AI.Domain.Model.Queries;
using SoftFocusBackend.AI.Domain.Services;

namespace SoftFocusBackend.AI.Application.Internal.QueryServices;

public class AIUsageQueryService
{
    private readonly IAIUsageTracker _usageTracker;
    private readonly ILogger<AIUsageQueryService> _logger;

    public AIUsageQueryService(IAIUsageTracker usageTracker, ILogger<AIUsageQueryService> logger)
    {
        _usageTracker = usageTracker;
        _logger = logger;
    }

    public async Task<AIUsage> HandleGetUsageStatsAsync(GetAIUsageStatsQuery query)
    {
        try
        {
            _logger.LogInformation("Getting AI usage stats for user {UserId}", query.UserId);
            return await _usageTracker.GetCurrentUsageAsync(query.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage stats for user {UserId}", query.UserId);
            throw;
        }
    }
}
