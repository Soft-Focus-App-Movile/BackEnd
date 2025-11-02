using SoftFocusBackend.Subscription.Domain.ValueObjects;
using SoftFocusBackend.Users.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Subscription.Application.DTOs;

public class SubscriptionDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public UserType UserType { get; set; }
    public SubscriptionPlan Plan { get; set; }
    public SubscriptionStatus Status { get; set; }
    public DateTime? CurrentPeriodStart { get; set; }
    public DateTime? CurrentPeriodEnd { get; set; }
    public bool CancelAtPeriodEnd { get; set; }
    public DateTime? CancelledAt { get; set; }
    public bool IsActive { get; set; }
    public UsageLimitsDto UsageLimits { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UsageLimitsDto
{
    public int? AiChatMessagesPerDay { get; set; }
    public int? FacialAnalysisPerWeek { get; set; }
    public int? ContentRecommendationsPerWeek { get; set; }
    public int? CheckInsPerDay { get; set; }
    public int? MaxPatientConnections { get; set; }
    public int? ContentAssignmentsPerWeek { get; set; }
}
