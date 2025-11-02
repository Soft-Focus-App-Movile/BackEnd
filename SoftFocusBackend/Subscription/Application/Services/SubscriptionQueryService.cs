using SoftFocusBackend.Subscription.Application.DTOs;
using SoftFocusBackend.Subscription.Application.Queries;
using SoftFocusBackend.Subscription.Domain.Aggregates;
using SoftFocusBackend.Subscription.Domain.ValueObjects;
using SoftFocusBackend.Subscription.Infrastructure.Repositories;

namespace SoftFocusBackend.Subscription.Application.Services;

public class SubscriptionQueryService : ISubscriptionQueryService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUsageTrackingRepository _usageTrackingRepository;
    private readonly ILogger<SubscriptionQueryService> _logger;

    public SubscriptionQueryService(
        ISubscriptionRepository subscriptionRepository,
        IUsageTrackingRepository usageTrackingRepository,
        ILogger<SubscriptionQueryService> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _usageTrackingRepository = usageTrackingRepository;
        _logger = logger;
    }

    public async Task<SubscriptionDto?> GetSubscriptionByUserIdAsync(GetSubscriptionByUserIdQuery query)
    {
        var subscription = await _subscriptionRepository.GetByUserIdAsync(query.UserId);
        return subscription == null ? null : MapToDto(subscription);
    }

    public async Task<AllUsageStatsDto> GetUsageStatsAsync(GetUsageStatsQuery query)
    {
        var subscription = await _subscriptionRepository.GetByUserIdAsync(query.UserId)
            ?? throw new InvalidOperationException("Subscription not found");

        var usageTrackings = await _usageTrackingRepository.GetAllByUserIdAsync(query.UserId);
        var limits = subscription.GetUsageLimits();

        var featureUsages = new List<UsageStatsDto>();

        // Map all features based on user type
        var relevantFeatures = GetRelevantFeatures(subscription.UserType);

        foreach (var featureType in relevantFeatures)
        {
            var tracking = usageTrackings.FirstOrDefault(t => t.FeatureType == featureType);
            var limit = GetLimitForFeature(limits, featureType);

            var usage = new UsageStatsDto
            {
                FeatureType = featureType,
                CurrentUsage = tracking?.UsageCount ?? 0,
                Limit = limit,
                IsUnlimited = limit == null,
                LimitReached = tracking != null && limit.HasValue && tracking.UsageCount >= limit.Value,
                Remaining = limit.HasValue ? Math.Max(0, limit.Value - (tracking?.UsageCount ?? 0)) : null,
                PeriodStart = tracking?.PeriodStart ?? DateTime.UtcNow,
                PeriodEnd = tracking?.PeriodEnd ?? DateTime.UtcNow.AddDays(1),
                LastUsedAt = tracking?.LastUsedAt
            };

            featureUsages.Add(usage);
        }

        return new AllUsageStatsDto
        {
            Plan = subscription.Plan,
            FeatureUsages = featureUsages,
            GeneratedAt = DateTime.UtcNow
        };
    }

    public async Task<FeatureAccessResponse> CheckFeatureAccessAsync(CheckFeatureAccessQuery query)
    {
        var subscription = await _subscriptionRepository.GetByUserIdAsync(query.UserId);
        if (subscription == null)
        {
            return new FeatureAccessResponse
            {
                HasAccess = false,
                DenialReason = "No subscription found",
                UpgradeMessage = "Please contact support"
            };
        }

        // Pro plan has unlimited access
        if (subscription.Plan == SubscriptionPlan.Pro && subscription.IsActive())
        {
            return new FeatureAccessResponse
            {
                HasAccess = true
            };
        }

        // Basic plan - check limits
        var limits = subscription.GetUsageLimits();
        var limit = GetLimitForFeature(limits, query.FeatureType);

        // If feature has no limit for this user type, allow access
        if (limit == null)
        {
            return new FeatureAccessResponse
            {
                HasAccess = true
            };
        }

        // Check current usage
        var tracking = await _usageTrackingRepository.GetByUserAndFeatureAsync(
            query.UserId,
            query.FeatureType);

        if (tracking == null)
        {
            // First time using this feature
            return new FeatureAccessResponse
            {
                HasAccess = true,
                CurrentUsage = 0,
                Limit = limit
            };
        }

        // Check if period has expired (reset usage)
        if (tracking.IsPeriodExpired())
        {
            return new FeatureAccessResponse
            {
                HasAccess = true,
                CurrentUsage = 0,
                Limit = limit
            };
        }

        // Check if limit is reached
        if (tracking.IsLimitReached(limit))
        {
            return new FeatureAccessResponse
            {
                HasAccess = false,
                DenialReason = $"You've reached your {GetFeatureName(query.FeatureType)} limit for this {GetPeriodName(query.FeatureType)}",
                CurrentUsage = tracking.UsageCount,
                Limit = limit,
                UpgradeMessage = "Upgrade to Pro for unlimited access. Only $12.99/month!"
            };
        }

        return new FeatureAccessResponse
        {
            HasAccess = true,
            CurrentUsage = tracking.UsageCount,
            Limit = limit
        };
    }

    private List<FeatureType> GetRelevantFeatures(Users.Domain.Model.ValueObjects.UserType userType)
    {
        return userType switch
        {
            Users.Domain.Model.ValueObjects.UserType.General => new List<FeatureType>
            {
                FeatureType.AiChatMessage,
                FeatureType.FacialAnalysis,
                FeatureType.ContentRecommendation,
                FeatureType.CheckIn
            },
            Users.Domain.Model.ValueObjects.UserType.Psychologist => new List<FeatureType>
            {
                FeatureType.PatientConnection,
                FeatureType.ContentAssignment
            },
            _ => new List<FeatureType>()
        };
    }

    private int? GetLimitForFeature(UsageLimits limits, FeatureType featureType)
    {
        return featureType switch
        {
            FeatureType.AiChatMessage => limits.AiChatMessagesPerDay,
            FeatureType.FacialAnalysis => limits.FacialAnalysisPerWeek,
            FeatureType.ContentRecommendation => limits.ContentRecommendationsPerWeek,
            FeatureType.CheckIn => limits.CheckInsPerDay,
            FeatureType.PatientConnection => limits.MaxPatientConnections,
            FeatureType.ContentAssignment => limits.ContentAssignmentsPerWeek,
            _ => null
        };
    }

    private string GetFeatureName(FeatureType featureType)
    {
        return featureType switch
        {
            FeatureType.AiChatMessage => "AI chat",
            FeatureType.FacialAnalysis => "facial analysis",
            FeatureType.ContentRecommendation => "content recommendation",
            FeatureType.CheckIn => "check-in",
            FeatureType.PatientConnection => "patient connection",
            FeatureType.ContentAssignment => "content assignment",
            _ => "feature"
        };
    }

    private string GetPeriodName(FeatureType featureType)
    {
        return featureType switch
        {
            FeatureType.AiChatMessage => "day",
            FeatureType.CheckIn => "day",
            FeatureType.FacialAnalysis => "week",
            FeatureType.ContentRecommendation => "week",
            FeatureType.ContentAssignment => "week",
            FeatureType.PatientConnection => "plan",
            _ => "period"
        };
    }

    private SubscriptionDto MapToDto(Domain.Aggregates.Subscription subscription)
    {
        var limits = subscription.GetUsageLimits();

        return new SubscriptionDto
        {
            Id = subscription.Id,
            UserId = subscription.UserId,
            UserType = subscription.UserType,
            Plan = subscription.Plan,
            Status = subscription.Status,
            CurrentPeriodStart = subscription.CurrentPeriodStart,
            CurrentPeriodEnd = subscription.CurrentPeriodEnd,
            CancelAtPeriodEnd = subscription.CancelAtPeriodEnd,
            CancelledAt = subscription.CancelledAt,
            IsActive = subscription.IsActive(),
            UsageLimits = new UsageLimitsDto
            {
                AiChatMessagesPerDay = limits.AiChatMessagesPerDay,
                FacialAnalysisPerWeek = limits.FacialAnalysisPerWeek,
                ContentRecommendationsPerWeek = limits.ContentRecommendationsPerWeek,
                CheckInsPerDay = limits.CheckInsPerDay,
                MaxPatientConnections = limits.MaxPatientConnections,
                ContentAssignmentsPerWeek = limits.ContentAssignmentsPerWeek
            },
            CreatedAt = subscription.CreatedAt,
            UpdatedAt = subscription.UpdatedAt
        };
    }
}
