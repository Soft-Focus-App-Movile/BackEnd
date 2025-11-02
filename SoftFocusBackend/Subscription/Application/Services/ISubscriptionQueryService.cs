using SoftFocusBackend.Subscription.Application.DTOs;
using SoftFocusBackend.Subscription.Application.Queries;
using SoftFocusBackend.Subscription.Domain.ValueObjects;

namespace SoftFocusBackend.Subscription.Application.Services;

public interface ISubscriptionQueryService
{
    Task<SubscriptionDto?> GetSubscriptionByUserIdAsync(GetSubscriptionByUserIdQuery query);
    Task<AllUsageStatsDto> GetUsageStatsAsync(GetUsageStatsQuery query);
    Task<FeatureAccessResponse> CheckFeatureAccessAsync(CheckFeatureAccessQuery query);
}
