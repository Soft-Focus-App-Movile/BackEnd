// ════════════════════════════════════════════════════════════════════════════
//  TIPO: PRUEBA UNITARIA
//  Clase bajo prueba: SubscriptionQueryService
// ════════════════════════════════════════════════════════════════════════════
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SoftFocusBackend.Subscription.Application.Queries;
using SoftFocusBackend.Subscription.Application.Services;
using SoftFocusBackend.Subscription.Domain.ValueObjects;
using SoftFocusBackend.Subscription.Infrastructure.Repositories;
using SoftFocusBackend.Users.Domain.Model.ValueObjects;
using SubEntity = SoftFocusBackend.Subscription.Domain.Aggregates.Subscription;
using SubUsage  = SoftFocusBackend.Subscription.Domain.Aggregates.UsageTracking;

namespace SoftFocusBackend.Tests.UnitTests.Subscription;

public class SubscriptionQueryServiceTests
{
    // ─── colaboradores mockeados ──────────────────────────────────────────
    private readonly Mock<ISubscriptionRepository>             _subscriptionRepoMock  = new();
    private readonly Mock<IUsageTrackingRepository>            _usageTrackingRepoMock = new();
    private readonly Mock<ILogger<SubscriptionQueryService>>   _loggerMock            = new();

    private readonly SubscriptionQueryService _sut;

    public SubscriptionQueryServiceTests()
    {
        _sut = new SubscriptionQueryService(
            _subscriptionRepoMock.Object,
            _usageTrackingRepoMock.Object,
            _loggerMock.Object);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────

    private static SubEntity BuildBasicSubscription(string userId = "user-001", UserType userType = UserType.General)
        => SubEntity.CreateBasicSubscription(userId, userType);

    private static SubEntity BuildProSubscription(
        string userId           = "user-001",
        string stripeCustomerId = "cus-001",
        string stripeSubId      = "sub-001")
    {
        var sub = SubEntity.CreateBasicSubscription(userId, UserType.General);
        sub.UpgradeToPro(stripeCustomerId, stripeSubId, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1));
        return sub;
    }

    private static SubUsage BuildTracking(
        string userId           = "user-001",
        FeatureType featureType = FeatureType.AiChatMessage,
        int usageCount          = 2)
    {
        var tracking = new SubUsage(userId, featureType, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(23));
        for (var i = 0; i < usageCount; i++) tracking.IncrementUsage();
        return tracking;
    }

    // ─── GetSubscriptionByUserIdAsync — Escenarios ───────────────────────

    [Fact]
    public async Task GetSubscriptionByUserIdAsync_ExistingSubscription_ReturnsDto()
    {
        // Arrange
        var subscription = BuildBasicSubscription();
        var query        = new GetSubscriptionByUserIdQuery { UserId = "user-001" };

        _subscriptionRepoMock.Setup(r => r.GetByUserIdAsync("user-001")).ReturnsAsync(subscription);

        // Act
        var result = await _sut.GetSubscriptionByUserIdAsync(query);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be("user-001");
        result.Plan.Should().Be(SubscriptionPlan.Basic);
    }

    [Fact]
    public async Task GetSubscriptionByUserIdAsync_NoSubscription_ReturnsNull()
    {
        // Arrange
        var query = new GetSubscriptionByUserIdQuery { UserId = "user-without-subscription" };

        _subscriptionRepoMock
            .Setup(r => r.GetByUserIdAsync("user-without-subscription"))
            .ReturnsAsync((SubEntity?)null);

        // Act
        var result = await _sut.GetSubscriptionByUserIdAsync(query);

        // Assert
        result.Should().BeNull();
    }

    // ─── CheckFeatureAccessAsync — Escenarios ─────────────────────────────

    [Fact]
    public async Task CheckFeatureAccessAsync_NoSubscription_ReturnsDeniedAccess()
    {
        // Arrange
        var query = new CheckFeatureAccessQuery { UserId = "user-001", FeatureType = FeatureType.AiChatMessage };

        _subscriptionRepoMock.Setup(r => r.GetByUserIdAsync("user-001")).ReturnsAsync((SubEntity?)null);

        // Act
        var result = await _sut.CheckFeatureAccessAsync(query);

        // Assert
        result.HasAccess.Should().BeFalse();
        result.DenialReason.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CheckFeatureAccessAsync_ProPlan_Active_ReturnsHasAccess()
    {
        // Arrange
        var subscription = BuildProSubscription();
        var query        = new CheckFeatureAccessQuery { UserId = "user-001", FeatureType = FeatureType.AiChatMessage };

        _subscriptionRepoMock.Setup(r => r.GetByUserIdAsync("user-001")).ReturnsAsync(subscription);

        // Act
        var result = await _sut.CheckFeatureAccessAsync(query);

        // Assert
        result.HasAccess.Should().BeTrue();
        _usageTrackingRepoMock.Verify(r => r.GetByUserAndFeatureAsync(It.IsAny<string>(), It.IsAny<FeatureType>()), Times.Never);
    }

    [Fact]
    public async Task CheckFeatureAccessAsync_BasicPlan_LimitNotReached_ReturnsHasAccess()
    {
        // Arrange — Basic plan tiene 3 mensajes AI por día
        var subscription = BuildBasicSubscription();
        var tracking     = BuildTracking(usageCount: 2); // 2 de 3
        var query        = new CheckFeatureAccessQuery { UserId = "user-001", FeatureType = FeatureType.AiChatMessage };

        _subscriptionRepoMock.Setup(r => r.GetByUserIdAsync("user-001")).ReturnsAsync(subscription);
        _usageTrackingRepoMock
            .Setup(r => r.GetByUserAndFeatureAsync("user-001", FeatureType.AiChatMessage))
            .ReturnsAsync(tracking);

        // Act
        var result = await _sut.CheckFeatureAccessAsync(query);

        // Assert
        result.HasAccess.Should().BeTrue();
        result.CurrentUsage.Should().Be(2);
        result.Limit.Should().Be(3);
    }

    [Fact]
    public async Task CheckFeatureAccessAsync_BasicPlan_LimitReached_ReturnsDeniedAccess()
    {
        // Arrange — Basic plan tiene 3 mensajes AI por día; el usuario ya usó 3
        var subscription = BuildBasicSubscription();
        var tracking     = BuildTracking(usageCount: 3); // 3 de 3 → límite alcanzado
        var query        = new CheckFeatureAccessQuery { UserId = "user-001", FeatureType = FeatureType.AiChatMessage };

        _subscriptionRepoMock.Setup(r => r.GetByUserIdAsync("user-001")).ReturnsAsync(subscription);
        _usageTrackingRepoMock
            .Setup(r => r.GetByUserAndFeatureAsync("user-001", FeatureType.AiChatMessage))
            .ReturnsAsync(tracking);

        // Act
        var result = await _sut.CheckFeatureAccessAsync(query);

        // Assert
        result.HasAccess.Should().BeFalse();
        result.DenialReason.Should().NotBeNullOrEmpty();
        result.UpgradeMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CheckFeatureAccessAsync_BasicPlan_FirstTimeUse_ReturnsHasAccess()
    {
        // Arrange — primera vez que el usuario usa la funcionalidad (sin tracking)
        var subscription = BuildBasicSubscription();
        var query        = new CheckFeatureAccessQuery { UserId = "user-001", FeatureType = FeatureType.FacialAnalysis };

        _subscriptionRepoMock.Setup(r => r.GetByUserIdAsync("user-001")).ReturnsAsync(subscription);
        _usageTrackingRepoMock
            .Setup(r => r.GetByUserAndFeatureAsync("user-001", FeatureType.FacialAnalysis))
            .ReturnsAsync((SubUsage?)null);

        // Act
        var result = await _sut.CheckFeatureAccessAsync(query);

        // Assert
        result.HasAccess.Should().BeTrue();
        result.CurrentUsage.Should().Be(0);
    }

    // ─── GetUsageStatsAsync — Escenarios ─────────────────────────────────

    [Fact]
    public async Task GetUsageStatsAsync_ExistingSubscription_ReturnsStats()
    {
        // Arrange
        var subscription = BuildBasicSubscription();
        var trackings    = new List<SubUsage>
        {
            BuildTracking(featureType: FeatureType.AiChatMessage, usageCount: 1),
            BuildTracking(featureType: FeatureType.FacialAnalysis, usageCount: 0)
        };
        var query = new GetUsageStatsQuery { UserId = "user-001" };

        _subscriptionRepoMock.Setup(r => r.GetByUserIdAsync("user-001")).ReturnsAsync(subscription);
        _usageTrackingRepoMock.Setup(r => r.GetAllByUserIdAsync("user-001")).ReturnsAsync(trackings);

        // Act
        var result = await _sut.GetUsageStatsAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Plan.Should().Be(SubscriptionPlan.Basic);
        result.FeatureUsages.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetUsageStatsAsync_NoSubscription_ThrowsInvalidOperationException()
    {
        // Arrange
        var query = new GetUsageStatsQuery { UserId = "user-without-subscription" };

        _subscriptionRepoMock
            .Setup(r => r.GetByUserIdAsync("user-without-subscription"))
            .ReturnsAsync((SubEntity?)null);

        // Act
        var act = async () => await _sut.GetUsageStatsAsync(query);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Subscription not found*");
    }

    [Fact]
    public async Task GetUsageStatsAsync_PsychologistUser_ReturnsOnlyPsychologistFeatures()
    {
        // Arrange
        var subscription = BuildBasicSubscription("psych-001", UserType.Psychologist);
        var query        = new GetUsageStatsQuery { UserId = "psych-001" };

        _subscriptionRepoMock.Setup(r => r.GetByUserIdAsync("psych-001")).ReturnsAsync(subscription);
        _usageTrackingRepoMock.Setup(r => r.GetAllByUserIdAsync("psych-001")).ReturnsAsync(new List<SubUsage>());

        // Act
        var result = await _sut.GetUsageStatsAsync(query);

        // Assert
        result.FeatureUsages.Should().OnlyContain(f =>
            f.FeatureType == FeatureType.PatientConnection ||
            f.FeatureType == FeatureType.ContentAssignment);
    }
}
