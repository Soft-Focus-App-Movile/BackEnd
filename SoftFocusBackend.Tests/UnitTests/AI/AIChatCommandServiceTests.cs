using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SoftFocusBackend.AI.Application.ACL.Services;
using SoftFocusBackend.AI.Application.Internal.CommandServices;
using SoftFocusBackend.AI.Domain.Model.Aggregates;
using SoftFocusBackend.AI.Domain.Model.Commands;
using SoftFocusBackend.AI.Domain.Model.ValueObjects;
using SoftFocusBackend.AI.Domain.Repositories;
using SoftFocusBackend.AI.Domain.Services;

namespace SoftFocusBackend.Tests.UnitTests.AI;

public class AIChatCommandServiceTests
{
    private readonly Mock<IAIUsageTracker>              _usageTrackerMock      = new();
    private readonly Mock<IGeminiContextBuilder>         _contextBuilderMock    = new();
    private readonly Mock<IEmotionalChatService>         _chatServiceMock       = new();
    private readonly Mock<ICrisisPatternDetector>        _crisisDetectorMock    = new();
    private readonly Mock<ICrisisIntegrationService>     _crisisIntegrationMock = new();
    private readonly Mock<IChatSessionRepository>        _sessionRepoMock       = new();
    private readonly Mock<ILogger<AIChatCommandService>> _loggerMock            = new();

    private readonly AIChatCommandService _sut;

    public AIChatCommandServiceTests()
    {
        _sut = new AIChatCommandService(
            _usageTrackerMock.Object,
            _contextBuilderMock.Object,
            _chatServiceMock.Object,
            _crisisDetectorMock.Object,
            _crisisIntegrationMock.Object,
            _sessionRepoMock.Object,
            _loggerMock.Object);
    }

    private static ChatSession BuildSession(string userId = "user-001") =>
        ChatSession.Create(userId);

    private static GeminiContext BuildContext(string userId = "user-001") =>
        new GeminiContext(userId, "Hola, ¿cómo estás?");

    private static ChatResponse BuildChatResponse(bool crisisDetected = false) => new()
    {
        Reply = "Estoy aquí para ayudarte.",
        SuggestedQuestions = new List<string> { "¿Cómo te sientes hoy?" },
        RecommendedExercises = new List<string>(),
        CrisisDetected = crisisDetected,
        CrisisContext = crisisDetected ? "Usuario expresó malestar intenso" : string.Empty
    };

    private void SetupHappyPath(string userId = "user-001")
    {
        var session  = BuildSession(userId);
        var context  = BuildContext(userId);
        var response = BuildChatResponse();

        _usageTrackerMock.Setup(u => u.CanUseChatAsync(userId)).ReturnsAsync(true);
        _sessionRepoMock.Setup(r => r.CreateAsync(userId)).ReturnsAsync(session);
        _sessionRepoMock.Setup(r => r.AddMessageAsync(It.IsAny<string>(), It.IsAny<ChatMessage>())).Returns(Task.CompletedTask);
        _contextBuilderMock.Setup(c => c.BuildContextAsync(userId, It.IsAny<string>(), null)).ReturnsAsync(context);
        _chatServiceMock.Setup(s => s.SendMessageAsync(It.IsAny<GeminiContext>())).ReturnsAsync(response);
        _crisisDetectorMock.Setup(d => d.DetectFromChatAsync(It.IsAny<ChatMessage>())).ReturnsAsync((CrisisPattern?)null);
        _usageTrackerMock.Setup(u => u.IncrementChatUsageAsync(userId)).Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task HandleSendMessageAsync_ValidCommand_ReturnsSuccess()
    {
        SetupHappyPath();
        var command = new SendChatMessageCommand("user-001", "Hola, ¿cómo estás?");

        var result = await _sut.HandleSendMessageAsync(command);

        result.IsSuccess.Should().BeTrue();
        result.IsLimitExceeded.Should().BeFalse();
        result.Response.Should().NotBeNull();
        result.Response!.Reply.Should().Be("Estoy aquí para ayudarte.");
    }

    [Fact]
    public async Task HandleSendMessageAsync_ValidCommand_PersistsUserAndAssistantMessages()
    {
        SetupHappyPath();
        var command = new SendChatMessageCommand("user-001", "Hola");

        await _sut.HandleSendMessageAsync(command);

        _sessionRepoMock.Verify(r => r.AddMessageAsync(It.IsAny<string>(), It.IsAny<ChatMessage>()), Times.Exactly(2));
    }

    [Fact]
    public async Task HandleSendMessageAsync_ValidCommand_IncrementsUsage()
    {
        SetupHappyPath();
        var command = new SendChatMessageCommand("user-001", "Hola");

        await _sut.HandleSendMessageAsync(command);

        _usageTrackerMock.Verify(u => u.IncrementChatUsageAsync("user-001"), Times.Once);
    }

    [Fact]
    public async Task HandleSendMessageAsync_NoCrisisDetected_DoesNotTriggerAlert()
    {
        SetupHappyPath();
        var command = new SendChatMessageCommand("user-001", "Hoy tuve un buen día");

        await _sut.HandleSendMessageAsync(command);

        _crisisIntegrationMock.Verify(c => c.TriggerCrisisAlertAsync(It.IsAny<CrisisAlertRequest>()), Times.Never);
    }

    [Fact]
    public async Task HandleSendMessageAsync_UsageLimitExceeded_ReturnsLimitExceeded()
    {
        var usage = AIUsage.Create("user-001", "2026-W20", DateTime.UtcNow, "Free", 10, 5);
        _usageTrackerMock.Setup(u => u.CanUseChatAsync("user-001")).ReturnsAsync(false);
        _usageTrackerMock.Setup(u => u.GetCurrentUsageAsync("user-001")).ReturnsAsync(usage);

        var command = new SendChatMessageCommand("user-001", "Hola");

        var result = await _sut.HandleSendMessageAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.IsLimitExceeded.Should().BeTrue();
        result.Usage.Should().NotBeNull();
    }

    [Fact]
    public async Task HandleSendMessageAsync_UsageLimitExceeded_NeverCallsChatService()
    {
        var usage = AIUsage.Create("user-001", "2026-W20", DateTime.UtcNow, "Free", 10, 5);
        _usageTrackerMock.Setup(u => u.CanUseChatAsync("user-001")).ReturnsAsync(false);
        _usageTrackerMock.Setup(u => u.GetCurrentUsageAsync("user-001")).ReturnsAsync(usage);

        var command = new SendChatMessageCommand("user-001", "Hola");

        await _sut.HandleSendMessageAsync(command);

        _chatServiceMock.Verify(s => s.SendMessageAsync(It.IsAny<GeminiContext>()), Times.Never);
    }

    [Fact]
    public async Task HandleSendMessageAsync_CrisisDetectedByGemini_TriggersAlert()
    {
        var session  = BuildSession();
        var context  = BuildContext();
        var response = BuildChatResponse(crisisDetected: true);

        _usageTrackerMock.Setup(u => u.CanUseChatAsync("user-001")).ReturnsAsync(true);
        _sessionRepoMock.Setup(r => r.CreateAsync("user-001")).ReturnsAsync(session);
        _sessionRepoMock.Setup(r => r.AddMessageAsync(It.IsAny<string>(), It.IsAny<ChatMessage>())).Returns(Task.CompletedTask);
        _contextBuilderMock.Setup(c => c.BuildContextAsync("user-001", It.IsAny<string>(), null)).ReturnsAsync(context);
        _chatServiceMock.Setup(s => s.SendMessageAsync(It.IsAny<GeminiContext>())).ReturnsAsync(response);
        _crisisDetectorMock.Setup(d => d.DetectFromChatAsync(It.IsAny<ChatMessage>())).ReturnsAsync((CrisisPattern?)null);
        _usageTrackerMock.Setup(u => u.IncrementChatUsageAsync("user-001")).Returns(Task.CompletedTask);
        _crisisIntegrationMock.Setup(c => c.TriggerCrisisAlertAsync(It.IsAny<CrisisAlertRequest>())).Returns(Task.CompletedTask);

        var command = new SendChatMessageCommand("user-001", "Me siento muy mal y no quiero continuar");

        await _sut.HandleSendMessageAsync(command);

        _crisisIntegrationMock.Verify(c => c.TriggerCrisisAlertAsync(
            It.Is<CrisisAlertRequest>(r => r.UserId == "user-001" && r.Source == "ai_chat")),
            Times.Once);
    }

    [Fact]
    public void SendChatMessageCommand_EmptyUserId_ThrowsArgumentException()
    {
        var act = () => new SendChatMessageCommand("", "Hola");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SendChatMessageCommand_EmptyMessage_ThrowsArgumentException()
    {
        var act = () => new SendChatMessageCommand("user-001", "");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SendChatMessageCommand_MessageOver2000Chars_ThrowsArgumentException()
    {
        var longMessage = new string('a', 2001);
        var act = () => new SendChatMessageCommand("user-001", longMessage);
        act.Should().Throw<ArgumentException>();
    }
}