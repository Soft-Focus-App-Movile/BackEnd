// ════════════════════════════════════════════════════════════════════════════
//  TIPO: PRUEBA UNITARIA
//  Clase bajo prueba: SendChatMessageCommandService
//  Estrategia: se mockean todos los colaboradores (repositorios, bus, logger)
//              y se verifica el comportamiento del servicio de forma aislada.
// ════════════════════════════════════════════════════════════════════════════
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SoftFocusBackend.Shared.Infrastructure.Events;
using SoftFocusBackend.Tests.Helpers;
using SoftFocusBackend.Therapy.Application.Internal.CommandServices;
using SoftFocusBackend.Therapy.Domain.Model.Commands;
using SoftFocusBackend.Therapy.Domain.Model.Events;
using SoftFocusBackend.Therapy.Domain.Repositories;
using SoftFocusBackend.Therapy.Domain.Services;

namespace SoftFocusBackend.Tests.UnitTests.Therapy;

public class SendChatMessageCommandServiceTests
{
    // ─── colaboradores mockeados ──────────────────────────────────────────
    private readonly Mock<IChatMessageRepository>              _messageRepoMock       = new();
    private readonly Mock<IChatModerationService>              _moderationMock        = new();
    private readonly Mock<ITherapeuticRelationshipRepository>  _relationshipRepoMock  = new();
    private readonly Mock<IDomainEventBus>                     _eventBusMock          = new();
    private readonly Mock<ILogger<SendChatMessageCommandService>> _loggerMock          = new();

    private readonly SendChatMessageCommandService _sut;

    public SendChatMessageCommandServiceTests()
    {
        _sut = new SendChatMessageCommandService(
            _messageRepoMock.Object,
            _moderationMock.Object,
            _relationshipRepoMock.Object,
            _eventBusMock.Object,
            _loggerMock.Object);

        // El servicio de moderación devuelve el contenido sin cambios por defecto
        _moderationMock
            .Setup(m => m.ModerateContentAsync(It.IsAny<SoftFocusBackend.Therapy.Domain.Model.ValueObjects.MessageContent>()))
            .ReturnsAsync((SoftFocusBackend.Therapy.Domain.Model.ValueObjects.MessageContent c) => c);
    }

    // ─── Escenarios felices ───────────────────────────────────────────────

    [Fact]
    public async Task Handle_ValidCommand_PersistsMessageAndReturnsIt()
    {
        // Arrange
        var relationship = DomainObjectBuilder.CreateActiveRelationship();
        var command = BuildCommand(senderId: relationship.PsychologistId);

        _relationshipRepoMock
            .Setup(r => r.GetByIdAsync(command.RelationshipId))
            .ReturnsAsync(relationship);

        _messageRepoMock
            .Setup(r => r.AddAsync(It.IsAny<SoftFocusBackend.Therapy.Domain.Model.Aggregates.ChatMessage>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command);

        // Assert
        result.Should().NotBeNull();
        result.SenderId.Should().Be(command.SenderId);
        result.ReceiverId.Should().Be(command.ReceiverId);
        result.Content.Value.Should().Be(command.Content);
        _messageRepoMock.Verify(r => r.AddAsync(It.IsAny<SoftFocusBackend.Therapy.Domain.Model.Aggregates.ChatMessage>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesDomainEvent()
    {
        // Arrange
        var relationship = DomainObjectBuilder.CreateActiveRelationship();
        var command = BuildCommand(senderId: relationship.PsychologistId);

        _relationshipRepoMock
            .Setup(r => r.GetByIdAsync(command.RelationshipId))
            .ReturnsAsync(relationship);

        _messageRepoMock
            .Setup(r => r.AddAsync(It.IsAny<SoftFocusBackend.Therapy.Domain.Model.Aggregates.ChatMessage>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.Handle(command);

        // Assert — el evento MessageSentEvent debe publicarse exactamente una vez
        _eventBusMock.Verify(
            b => b.PublishAsync(It.Is<MessageSentEvent>(e =>
                e.SenderId   == command.SenderId &&
                e.ReceiverId == command.ReceiverId)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_SenderIsPsychologist_EventHasSenderIsPsychologistTrue()
    {
        // Arrange
        var relationship = DomainObjectBuilder.CreateActiveRelationship();
        // El psicólogo es quien envía
        var command = BuildCommand(
            relationshipId: relationship.Id,
            senderId:       relationship.PsychologistId,
            receiverId:     relationship.PatientId);

        _relationshipRepoMock.Setup(r => r.GetByIdAsync(command.RelationshipId)).ReturnsAsync(relationship);
        _messageRepoMock.Setup(r => r.AddAsync(It.IsAny<SoftFocusBackend.Therapy.Domain.Model.Aggregates.ChatMessage>())).Returns(Task.CompletedTask);

        // Act
        await _sut.Handle(command);

        // Assert
        _eventBusMock.Verify(
            b => b.PublishAsync(It.Is<MessageSentEvent>(e => e.SenderIsPsychologist == true)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_SenderIsPatient_EventHasSenderIsPsychologistFalse()
    {
        // Arrange
        var relationship = DomainObjectBuilder.CreateActiveRelationship();
        var command = BuildCommand(
            senderId:   relationship.PatientId,
            receiverId: relationship.PsychologistId);

        _relationshipRepoMock.Setup(r => r.GetByIdAsync(command.RelationshipId)).ReturnsAsync(relationship);
        _messageRepoMock.Setup(r => r.AddAsync(It.IsAny<SoftFocusBackend.Therapy.Domain.Model.Aggregates.ChatMessage>())).Returns(Task.CompletedTask);

        // Act
        await _sut.Handle(command);

        // Assert
        _eventBusMock.Verify(
            b => b.PublishAsync(It.Is<MessageSentEvent>(e => e.SenderIsPsychologist == false)),
            Times.Once);
    }

    // ─── Escenarios de error ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_RelationshipNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        _relationshipRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((SoftFocusBackend.Therapy.Domain.Model.Aggregates.TherapeuticRelationship?)null);

        var command = BuildCommand();

        // Act
        var act = () => _sut.Handle(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Invalid or inactive relationship*");
    }

    [Fact]
    public async Task Handle_InactiveRelationship_ThrowsInvalidOperationException()
    {
        // Arrange
        var relationship = DomainObjectBuilder.CreateTerminatedRelationship(); // IsActive = false
        _relationshipRepoMock
            .Setup(r => r.GetByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(relationship);

        var command = BuildCommand();

        // Act
        var act = () => _sut.Handle(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_EventBusFails_DoesNotPropagateException()
    {
        // Arrange — el bus lanza excepción pero el flujo principal NO debe romperse
        var relationship = DomainObjectBuilder.CreateActiveRelationship();
        var command = BuildCommand(senderId: relationship.PsychologistId);

        _relationshipRepoMock.Setup(r => r.GetByIdAsync(command.RelationshipId)).ReturnsAsync(relationship);
        _messageRepoMock.Setup(r => r.AddAsync(It.IsAny<SoftFocusBackend.Therapy.Domain.Model.Aggregates.ChatMessage>())).Returns(Task.CompletedTask);
        _eventBusMock.Setup(b => b.PublishAsync(It.IsAny<MessageSentEvent>()))
            .ThrowsAsync(new Exception("Bus down"));

        // Act
        var act = () => _sut.Handle(command);

        // Assert — no se propaga la excepción del bus
        await act.Should().NotThrowAsync();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Handle_EmptyContent_ThrowsArgumentException(string? content)
    {
        // Arrange
        var relationship = DomainObjectBuilder.CreateActiveRelationship();
        _relationshipRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(relationship);

        var command = BuildCommand(content: content!);

        // Act
        var act = () => _sut.Handle(command);

        // Assert — MessageContent.Create lanza ArgumentException para contenido vacío
        await act.Should().ThrowAsync<ArgumentException>();
    }

    // ─── helper ──────────────────────────────────────────────────────────

    private static SendChatMessageCommand BuildCommand(
        string? relationshipId = null,
        string? senderId       = null,
        string? receiverId     = null,
        string  content        = "Hola, ¿cómo te sientes hoy?",
        string  messageType    = "text") => new()
    {
        RelationshipId = relationshipId ?? DomainObjectBuilder.RelationshipId,
        SenderId       = senderId       ?? DomainObjectBuilder.PsychologistId,
        ReceiverId     = receiverId     ?? DomainObjectBuilder.PatientId,
        Content        = content,
        MessageType    = messageType
    };
}