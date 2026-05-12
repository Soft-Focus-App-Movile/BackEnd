// ════════════════════════════════════════════════════════════════════════════
//  TIPO: PRUEBA UNITARIA
//  Clase bajo prueba: ChatHistoryQueryService
// ════════════════════════════════════════════════════════════════════════════
using FluentAssertions;
using Moq;
using SoftFocusBackend.Therapy.Application.Internal.QueryServices;
using SoftFocusBackend.Therapy.Domain.Model.Aggregates;
using SoftFocusBackend.Therapy.Domain.Model.Queries;
using SoftFocusBackend.Therapy.Domain.Model.ValueObjects;
using SoftFocusBackend.Therapy.Domain.Repositories;

namespace SoftFocusBackend.Tests.UnitTests.Therapy;

public class ChatHistoryQueryServiceTests
{
    private readonly Mock<IChatMessageRepository> _messageRepoMock = new();
    private readonly ChatHistoryQueryService _sut;

    public ChatHistoryQueryServiceTests()
    {
        _sut = new ChatHistoryQueryService(_messageRepoMock.Object);
    }

    [Fact]
    public async Task Handle_GetChatHistoryQuery_ReturnsMessages()
    {
        // Arrange
        var query = new GetChatHistoryQuery { RelationshipId = "rel1", Page = 1, Size = 10 };
        var expectedMessage = new ChatMessage("rel1", "sender1", "receiver1", MessageContent.Create("Hola"), "text");
        
        _messageRepoMock
            .Setup(r => r.GetByRelationshipIdAsync("rel1", 1, 10))
            .ReturnsAsync(new List<ChatMessage> { expectedMessage });

        // Act
        var result = await _sut.Handle(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Content.Value.Should().Be("Hola");
        _messageRepoMock.Verify(r => r.GetByRelationshipIdAsync("rel1", 1, 10), Times.Once);
    }

    [Fact]
    public async Task Handle_GetLastMessageQuery_ReturnsLastMessage()
    {
        // Arrange
        var query = new GetLastMessageQuery { ReceiverId = "receiver1" };
        var expectedMessage = new ChatMessage("rel1", "sender1", "receiver1", MessageContent.Create("Último mensaje"), "text");
        
        _messageRepoMock
            .Setup(r => r.GetLastMessageByReceiverIdAsync("receiver1"))
            .ReturnsAsync(expectedMessage);

        // Act
        var result = await _sut.Handle(query);

        // Assert
        result.Should().NotBeNull();
        result!.Content.Value.Should().Be("Último mensaje");
    }
}