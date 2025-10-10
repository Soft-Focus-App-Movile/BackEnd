using SoftFocusBackend.AI.Domain.Model.ValueObjects;

namespace SoftFocusBackend.AI.Application.ACL.Services;

public interface ISubscriptionIntegrationService
{
    Task<string> GetUserPlanAsync(string userId);
    Task<AIUsageLimit> GetPlanLimitsAsync(string plan);
}
