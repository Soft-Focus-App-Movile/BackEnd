using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SoftFocusBackend.AI.Application.Internal.QueryServices;
using SoftFocusBackend.AI.Domain.Model.Aggregates;
using SoftFocusBackend.AI.Domain.Model.Queries;
using SoftFocusBackend.AI.Domain.Repositories;
using SoftFocusBackend.AI.Domain.Services;

namespace SoftFocusBackend.Tests.UnitTests.AI;

public class AIQueryServiceTests
{
    private readonly Mock<IChatSessionRepository>         _sessionRepoMock    = new();
    private readonly Mock<IAIUsageTracker>                _usageTrackerMock   = new();
    private readonly Mock<ILogger<AIChatQueryService>>    _chatLoggerMock     = new();
    private readonly Mock<ILogger<AIUsageQueryService>>   _usageLoggerMock    = new();

    private readonly AIChatQueryService  _chatSut;
    private readonly AIUsageQueryService _usageSut;

    public AIQueryServiceTests()
    {
        _chatSut  = new AIChatQueryService(_sessionRepoMock.Object, _chatLoggerMock.Object);
        _usageSut = new AIUsageQueryService(_usageTrackerMock.Object, _usageLoggerMock.Object);
    }

    private static ChatSession BuildSession(string userId = "user-001") =>
        ChatSession.Create(userId);

    private static AIUsage BuildUsage(string userId = "user-001") =>
        AIUsage.Create(userId, "2026-W20", DateTime.UtcNow, "Free", 10, 5);

    [Fact]
    public async Task HandleGetUserChatHistoryAsync_ExistingUser_ReturnsSessions()
    {
        var sessions = new List<ChatSession> { BuildSession(), BuildSession() };
        var query    = new GetUserChatHistoryQuery("user-001");

        _sessionRepoMock
            .Setup(r => r.GetUserSessionsAsync("user-001", null, null, It.IsAny<int>()))
            .ReturnsAsync(sessions);

        var result = await _chatSut.HandleGetUserChatHistoryAsync(query);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleGetUserChatHistoryAsync_NoSessions_ReturnsEmptyList()
    {
        var query = new GetUserChatHistoryQuery("user-001");

        _sessionRepoMock
            .Setup(r => r.GetUserSessionsAsync("user-001", null, null, It.IsAny<int>()))
            .ReturnsAsync(new List<ChatSession>());

        var result = await _chatSut.HandleGetUserChatHistoryAsync(query);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleGetChatSessionAsync_ExistingSession_ReturnsSession()
    {
        var session = BuildSession();
        var query   = new GetChatSessionQuery("session-001", "user-001");

        _sessionRepoMock
            .Setup(r => r.GetByIdAsync("session-001"))
            .ReturnsAsync(session);

        var result = await _chatSut.HandleGetChatSessionAsync(query);

        result.Should().NotBeNull();
        result!.UserId.Should().Be("user-001");
    }

    [Fact]
    public async Task HandleGetChatSessionAsync_NonExistentSession_ReturnsNull()
    {
        var query = new GetChatSessionQuery("no-existe", "user-001");

        _sessionRepoMock
            .Setup(r => r.GetByIdAsync("no-existe"))
            .ReturnsAsync((ChatSession?)null);

        var result = await _chatSut.HandleGetChatSessionAsync(query);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetSessionMessagesAsync_ExistingSession_ReturnsMessages()
    {
        var messages = new List<ChatMessage>
        {
            ChatMessage.CreateUserMessage("session-001", "Hola"),
            ChatMessage.CreateAssistantMessage("session-001", "Hola, ¿en qué puedo ayudarte?", new(), new(), false)
        };

        _sessionRepoMock
            .Setup(r => r.GetSessionMessagesAsync("session-001", It.IsAny<int>()))
            .ReturnsAsync(messages);

        var result = await _chatSut.GetSessionMessagesAsync("session-001");

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleGetUsageStatsAsync_ValidUser_ReturnsUsage()
    {
        var usage = BuildUsage();
        var query = new GetAIUsageStatsQuery("user-001");

        _usageTrackerMock
            .Setup(u => u.GetCurrentUsageAsync("user-001"))
            .ReturnsAsync(usage);

        var result = await _usageSut.HandleGetUsageStatsAsync(query);

        result.Should().NotBeNull();
        result.UserId.Should().Be("user-001");
        result.Plan.Should().Be("Free");
    }

    [Fact]
    public async Task HandleGetUsageStatsAsync_ValidUser_CallsTrackerOnce()
    {
        var usage = BuildUsage();
        var query = new GetAIUsageStatsQuery("user-001");

        _usageTrackerMock.Setup(u => u.GetCurrentUsageAsync("user-001")).ReturnsAsync(usage);

        await _usageSut.HandleGetUsageStatsAsync(query);

        _usageTrackerMock.Verify(u => u.GetCurrentUsageAsync("user-001"), Times.Once);
    }

    [Fact]
    public void AIUsage_CanUseChat_WhenBelowLimit_ReturnsTrue()
    {
        var usage = BuildUsage();

        usage.CanUseChat().Should().BeTrue();
        usage.RemainingChatMessages().Should().Be(10);
    }

    [Fact]
    public void AIUsage_CanUseChat_WhenAtLimit_ReturnsFalse()
    {
        var usage = BuildUsage();
        for (var i = 0; i < 10; i++) usage.IncrementChatUsage();

        usage.CanUseChat().Should().BeFalse();
        usage.RemainingChatMessages().Should().Be(0);
    }
}