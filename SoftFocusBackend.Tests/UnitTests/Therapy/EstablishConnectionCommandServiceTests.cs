// ════════════════════════════════════════════════════════════════════════════
//  TIPO: PRUEBA UNITARIA
//  Clase bajo prueba: EstablishConnectionCommandService
// ════════════════════════════════════════════════════════════════════════════
using FluentAssertions;
using Moq;
using SoftFocusBackend.Tests.Helpers;
using SoftFocusBackend.Therapy.Application.Internal.CommandServices;
using SoftFocusBackend.Therapy.Domain.Model.Aggregates;
using SoftFocusBackend.Therapy.Domain.Model.Commands;
using SoftFocusBackend.Therapy.Domain.Model.ValueObjects;
using SoftFocusBackend.Therapy.Domain.Repositories;
using SoftFocusBackend.Therapy.Domain.Services;

namespace SoftFocusBackend.Tests.UnitTests.Therapy;

public class EstablishConnectionCommandServiceTests
{
    private readonly Mock<ITherapeuticRelationshipRepository> _repoMock       = new();
    private readonly Mock<IConnectionValidationService>       _validationMock  = new();
    private readonly EstablishConnectionCommandService        _sut;

    public EstablishConnectionCommandServiceTests()
    {
        _sut = new EstablishConnectionCommandService(
            _repoMock.Object,
            _validationMock.Object);
    }

    // ─── Escenarios felices ───────────────────────────────────────────────

    [Fact]
    public async Task Handle_ValidCode_PersistsAndReturnsRelationship()
    {
        // Arrange
        var expectedRelationship = DomainObjectBuilder.CreateActiveRelationship();

        _validationMock
            .Setup(v => v.EstablishConnectionAsync(
                It.IsAny<string>(),
                It.Is<ConnectionCode>(c => c.Value == "ABCD1234")))
            .ReturnsAsync(expectedRelationship);

        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<TherapeuticRelationship>()))
            .Returns(Task.CompletedTask);

        var command = new EstablishConnectionCommand
        {
            PatientId      = DomainObjectBuilder.PatientId,
            ConnectionCode = "ABCD1234"
        };

        // Act
        var result = await _sut.Handle(command);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(expectedRelationship);
        _repoMock.Verify(r => r.AddAsync(expectedRelationship), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCode_RelationshipIsActive()
    {
        // Arrange
        var relationship = DomainObjectBuilder.CreateActiveRelationship();

        _validationMock
            .Setup(v => v.EstablishConnectionAsync(It.IsAny<string>(), It.IsAny<ConnectionCode>()))
            .ReturnsAsync(relationship);

        _repoMock.Setup(r => r.AddAsync(It.IsAny<TherapeuticRelationship>())).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(new EstablishConnectionCommand
        {
            PatientId = DomainObjectBuilder.PatientId, ConnectionCode = "ABCD1234"
        });

        // Assert
        result.IsActive.Should().BeTrue();
        result.Status.Should().Be(TherapyStatus.Active);
    }

    // ─── Escenarios de error ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_InvalidCode_WrongLength_ThrowsArgumentException()
    {
        // Arrange — código con menos de 8 chars → ConnectionCode.Create lanza
        var command = new EstablishConnectionCommand
        {
            PatientId = DomainObjectBuilder.PatientId, ConnectionCode = "CORTO"
        };

        // Act
        var act = () => _sut.Handle(command);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*8 characters*");
    }

    [Fact]
    public async Task Handle_ExpiredCode_ThrowsInvalidOperationException()
    {
        // Arrange
        _validationMock
            .Setup(v => v.EstablishConnectionAsync(It.IsAny<string>(), It.IsAny<ConnectionCode>()))
            .ThrowsAsync(new InvalidOperationException("Invitation code has expired"));

        // Act
        var act = () => _sut.Handle(new EstablishConnectionCommand
        {
            PatientId = DomainObjectBuilder.PatientId, ConnectionCode = "EXPIRED1"
        });

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*expired*");
    }

    [Fact]
    public async Task Handle_PatientAlreadyHasActiveRelationship_ThrowsInvalidOperationException()
    {
        // Arrange
        _validationMock
            .Setup(v => v.EstablishConnectionAsync(It.IsAny<string>(), It.IsAny<ConnectionCode>()))
            .ThrowsAsync(new InvalidOperationException(
                "Patient already has an active therapeutic relationship"));

        // Act
        var act = () => _sut.Handle(new EstablishConnectionCommand
        {
            PatientId = DomainObjectBuilder.PatientId, ConnectionCode = "ABCD1234"
        });

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already has an active*");
    }

    [Fact]
    public async Task Handle_InvalidCodeString_DoesNotCallValidationService()
    {
        // Arrange — código vacío → falla antes de llegar al servicio de validación
        var command = new EstablishConnectionCommand
        {
            PatientId = DomainObjectBuilder.PatientId, ConnectionCode = ""
        };

        // Act
        try { await _sut.Handle(command); } catch { /* esperado */ }

        // Assert
        _validationMock.Verify(
            v => v.EstablishConnectionAsync(It.IsAny<string>(), It.IsAny<ConnectionCode>()),
            Times.Never);
    }
}