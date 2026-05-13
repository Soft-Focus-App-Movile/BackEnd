// ════════════════════════════════════════════════════════════════════════════
//  TIPO: PRUEBA UNITARIA
//  Clase bajo prueba: SubscriptionCommandService
// ════════════════════════════════════════════════════════════════════════════
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SoftFocusBackend.Subscription.Application.Commands;
using SoftFocusBackend.Subscription.Application.Services;
using SoftFocusBackend.Subscription.Domain.ValueObjects;
using SoftFocusBackend.Subscription.Infrastructure.ExternalServices;
using SoftFocusBackend.Subscription.Infrastructure.Repositories;
using SoftFocusBackend.Users.Domain.Model.ValueObjects;
using SoftFocusBackend.Users.Infrastructure.Persistence.MongoDB.Repositories;
using SubEntity = SoftFocusBackend.Subscription.Domain.Aggregates.Subscription;
using SubUsage = SoftFocusBackend.Subscription.Domain.Aggregates.UsageTracking;

namespace SoftFocusBackend.Tests.UnitTests.Subscription;

public class SubscriptionCommandServiceTests
{
    // ─── colaboradores mockeados ──────────────────────────────────────────
    private readonly Mock<ISubscriptionRepository>              _subscriptionRepoMock  = new();
    private readonly Mock<IUsageTrackingRepository>             _usageTrackingRepoMock = new();
    private readonly Mock<IStripePaymentService>                _stripeMock            = new();
    private readonly Mock<IUserRepository>                      _userRepoMock          = new();
    private readonly Mock<ILogger<SubscriptionCommandService>>  _loggerMock            = new();

    private readonly SubscriptionCommandService _sut;

    public SubscriptionCommandServiceTests()
    {
        _sut = new SubscriptionCommandService(
            _subscriptionRepoMock.Object,
            _usageTrackingRepoMock.Object,
            _stripeMock.Object,
            _userRepoMock.Object,
            _loggerMock.Object);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────

    private static SubEntity BuildBasicSubscription(
        string userId   = "user-001",
        UserType type   = UserType.General)
        => SubEntity.CreateBasicSubscription(userId, type);

    private static SubEntity BuildProSubscription(
        string userId             = "user-001",
        string stripeCustomerId   = "cus-001",
        string stripeSubId        = "sub-001")
    {
        var sub = SubEntity.CreateBasicSubscription(userId, UserType.General);
        sub.UpgradeToPro(stripeCustomerId, stripeSubId, DateTime.UtcNow, DateTime.UtcNow.AddMonths(1));
        return sub;
    }

    private static SubUsage BuildActiveTracking(
        string userId             = "user-001",
        FeatureType featureType   = FeatureType.AiChatMessage,
        int count                 = 1)
    {
        var tracking = new SubUsage(userId, featureType, DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(23));
        for (var i = 0; i < count; i++) tracking.IncrementUsage();
        return tracking;
    }

    private static SubUsage BuildExpiredTracking(
        string userId           = "user-001",
        FeatureType featureType = FeatureType.AiChatMessage)
    {
        return new SubUsage(userId, featureType, DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-1));
    }

    // ─── CreateBasicSubscriptionAsync — Escenarios ───────────────────────

    [Fact]
    public async Task CreateBasicSubscriptionAsync_NewUser_ReturnsSubscriptionDto()
    {
        // Arrange
        var command      = new CreateBasicSubscriptionCommand { UserId = "user-001", UserType = UserType.General };
        var subscription = BuildBasicSubscription();

        _subscriptionRepoMock.Setup(r => r.GetByUserIdAsync("user-001")).ReturnsAsync((SubEntity?)null);
        _subscriptionRepoMock.Setup(r => r.CreateAsync(It.IsAny<SubEntity>())).ReturnsAsync(subscription);

        // Act
        var result = await _sut.CreateBasicSubscriptionAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Plan.Should().Be(SubscriptionPlan.Basic);
        result.Status.Should().Be(SubscriptionStatus.Active);
    }

    [Fact]
    public async Task CreateBasicSubscriptionAsync_NewUser_CallsCreateAsyncOnce()
    {
        // Arrange
        var command      = new CreateBasicSubscriptionCommand { UserId = "user-001", UserType = UserType.General };
        var subscription = BuildBasicSubscription();

        _subscriptionRepoMock.Setup(r => r.GetByUserIdAsync("user-001")).ReturnsAsync((SubEntity?)null);
        _subscriptionRepoMock.Setup(r => r.CreateAsync(It.IsAny<SubEntity>())).ReturnsAsync(subscription);

        // Act
        await _sut.CreateBasicSubscriptionAsync(command);

        // Assert
        _subscriptionRepoMock.Verify(r => r.CreateAsync(It.IsAny<SubEntity>()), Times.Once);
    }

    [Fact]
    public async Task CreateBasicSubscriptionAsync_ExistingSubscription_ReturnsExistingWithoutCreating()
    {
        // Arrange
        var command      = new CreateBasicSubscriptionCommand { UserId = "user-001", UserType = UserType.General };
        var subscription = BuildBasicSubscription();

        _subscriptionRepoMock.Setup(r => r.GetByUserIdAsync("user-001")).ReturnsAsync(subscription);

        // Act
        var result = await _sut.CreateBasicSubscriptionAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be("user-001");
        _subscriptionRepoMock.Verify(r => r.CreateAsync(It.IsAny<SubEntity>()), Times.Never);
    }

    // ─── CancelSubscriptionAsync — Escenarios ────────────────────────────

    [Fact]
    public async Task CancelSubscriptionAsync_ProPlan_ImmediateCancellation_DowngradesToBasic()
    {
        // Arrange
        var subscription = BuildProSubscription("user-001", "cus-001", "sub-001");
        var command      = new CancelSubscriptionCommand { UserId = "user-001", CancelImmediately = true };

        _subscriptionRepoMock.Setup(r => r.GetByUserIdAsync("user-001")).ReturnsAsync(subscription);
        _stripeMock.Setup(s => s.CancelSubscriptionImmediatelyAsync("sub-001")).Returns(Task.CompletedTask);
        _subscriptionRepoMock.Setup(r => r.UpdateAsync(It.IsAny<SubEntity>())).ReturnsAsync(subscription);

        // Act
        var result = await _sut.CancelSubscriptionAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Plan.Should().Be(SubscriptionPlan.Basic);
        _stripeMock.Verify(s => s.CancelSubscriptionImmediatelyAsync("sub-001"), Times.Once);
    }

    [Fact]
    public async Task CancelSubscriptionAsync_ProPlan_AtEndOfPeriod_MarksCancelAtPeriodEnd()
    {
        // Arrange
        var subscription = BuildProSubscription("user-001", "cus-001", "sub-001");
        var command      = new CancelSubscriptionCommand { UserId = "user-001", CancelImmediately = false };

        _subscriptionRepoMock.Setup(r => r.GetByUserIdAsync("user-001")).ReturnsAsync(subscription);
        _stripeMock.Setup(s => s.CancelSubscriptionAsync("sub-001")).Returns(Task.CompletedTask);
        _subscriptionRepoMock.Setup(r => r.UpdateAsync(It.IsAny<SubEntity>())).ReturnsAsync(subscription);

        // Act
        var result = await _sut.CancelSubscriptionAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.CancelAtPeriodEnd.Should().BeTrue();
        _stripeMock.Verify(s => s.CancelSubscriptionAsync("sub-001"), Times.Once);
    }

    [Fact]
    public async Task CancelSubscriptionAsync_BasicPlan_ThrowsInvalidOperationException()
    {
        // Arrange
        var subscription = BuildBasicSubscription();
        var command      = new CancelSubscriptionCommand { UserId = "user-001", CancelImmediately = true };

        _subscriptionRepoMock.Setup(r => r.GetByUserIdAsync("user-001")).ReturnsAsync(subscription);

        // Act
        var act = async () => await _sut.CancelSubscriptionAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Basic plan*");
        _subscriptionRepoMock.Verify(r => r.UpdateAsync(It.IsAny<SubEntity>()), Times.Never);
    }

    [Fact]
    public async Task CancelSubscriptionAsync_SubscriptionNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = new CancelSubscriptionCommand { UserId = "unknown-user", CancelImmediately = false };

        _subscriptionRepoMock.Setup(r => r.GetByUserIdAsync("unknown-user")).ReturnsAsync((SubEntity?)null);

        // Act
        var act = async () => await _sut.CancelSubscriptionAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Subscription not found*");
    }

    // ─── TrackFeatureUsageAsync — Escenarios ─────────────────────────────

    [Fact]
    public async Task TrackFeatureUsageAsync_NoExistingTracking_CreatesNewRecord()
    {
        // Arrange
        var command  = new TrackFeatureUsageCommand { UserId = "user-001", FeatureType = FeatureType.AiChatMessage };
        var tracking = BuildActiveTracking();

        _usageTrackingRepoMock
            .Setup(r => r.GetByUserAndFeatureAsync("user-001", FeatureType.AiChatMessage))
            .ReturnsAsync((SubUsage?)null);
        _usageTrackingRepoMock.Setup(r => r.CreateAsync(It.IsAny<SubUsage>())).ReturnsAsync(tracking);

        // Act
        await _sut.TrackFeatureUsageAsync(command);

        // Assert
        _usageTrackingRepoMock.Verify(r => r.CreateAsync(It.IsAny<SubUsage>()), Times.Once);
        _usageTrackingRepoMock.Verify(r => r.UpdateAsync(It.IsAny<SubUsage>()), Times.Never);
    }

    [Fact]
    public async Task TrackFeatureUsageAsync_ExistingActiveTracking_UpdatesRecord()
    {
        // Arrange
        var command  = new TrackFeatureUsageCommand { UserId = "user-001", FeatureType = FeatureType.AiChatMessage };
        var tracking = BuildActiveTracking(count: 2);

        _usageTrackingRepoMock
            .Setup(r => r.GetByUserAndFeatureAsync("user-001", FeatureType.AiChatMessage))
            .ReturnsAsync(tracking);
        _usageTrackingRepoMock.Setup(r => r.UpdateAsync(tracking)).ReturnsAsync(tracking);

        // Act
        await _sut.TrackFeatureUsageAsync(command);

        // Assert
        _usageTrackingRepoMock.Verify(r => r.UpdateAsync(tracking), Times.Once);
        _usageTrackingRepoMock.Verify(r => r.CreateAsync(It.IsAny<SubUsage>()), Times.Never);
    }

    [Fact]
    public async Task TrackFeatureUsageAsync_ExpiredPeriod_ResetsPeriodAndUpdates()
    {
        // Arrange
        var command          = new TrackFeatureUsageCommand { UserId = "user-001", FeatureType = FeatureType.CheckIn };
        var expiredTracking  = BuildExpiredTracking();

        _usageTrackingRepoMock
            .Setup(r => r.GetByUserAndFeatureAsync("user-001", FeatureType.CheckIn))
            .ReturnsAsync(expiredTracking);
        _usageTrackingRepoMock.Setup(r => r.UpdateAsync(expiredTracking)).ReturnsAsync(expiredTracking);

        // Act
        await _sut.TrackFeatureUsageAsync(command);

        // Assert — el período expirado se restablece y se llama a Update, no a Create
        _usageTrackingRepoMock.Verify(r => r.UpdateAsync(expiredTracking), Times.Once);
        _usageTrackingRepoMock.Verify(r => r.CreateAsync(It.IsAny<SubUsage>()), Times.Never);
    }
}
