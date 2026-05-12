// ════════════════════════════════════════════════════════════════════════════
//  TIPO: PRUEBA UNITARIA
//  Clase bajo prueba: TerminateRelationshipCommandService
// ════════════════════════════════════════════════════════════════════════════
using FluentAssertions;
using Moq;
using SoftFocusBackend.Tests.Helpers;
using SoftFocusBackend.Therapy.Application.Internal.CommandServices;
using SoftFocusBackend.Therapy.Domain.Model.Commands;
using SoftFocusBackend.Therapy.Domain.Model.ValueObjects;
using SoftFocusBackend.Therapy.Domain.Repositories;

namespace SoftFocusBackend.Tests.UnitTests.Therapy;

public class TerminateRelationshipCommandServiceTests
{
    private readonly Mock<ITherapeuticRelationshipRepository> _repoMock = new();
    private readonly TerminateRelationshipCommandService      _sut;

    public TerminateRelationshipCommandServiceTests()
    {
        _sut = new TerminateRelationshipCommandService(_repoMock.Object);
    }

    // ─── Escenarios felices ───────────────────────────────────────────────

    [Fact]
    public async Task Handle_PsychologistTerminates_RelationshipIsTerminated()
    {
        // Arrange
        var relationship = DomainObjectBuilder.CreateActiveRelationship();

        _repoMock.Setup(r => r.GetByPatientIdAsync(It.IsAny<string>()))
            .ReturnsAsync(Enumerable.Empty<SoftFocusBackend.Therapy.Domain.Model.Aggregates.TherapeuticRelationship>());

        _repoMock.Setup(r => r.GetByPsychologistIdAsync(relationship.PsychologistId))
            .ReturnsAsync(new[] { relationship });

        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<SoftFocusBackend.Therapy.Domain.Model.Aggregates.TherapeuticRelationship>()))
            .Returns(Task.CompletedTask);

        var command = new TerminateRelationshipCommand
        {
            UserId         = relationship.PsychologistId,
            RelationshipId = relationship.Id
        };

        // Act
        await _sut.Handle(command);

        // Assert
        relationship.Status.Should().Be(TherapyStatus.Terminated);
        relationship.IsActive.Should().BeFalse();
        relationship.EndDate.Should().NotBeNull();
        _repoMock.Verify(r => r.UpdateAsync(relationship), Times.Once);
    }

    [Fact]
    public async Task Handle_PatientTerminates_RelationshipIsTerminated()
    {
        // Arrange
        var relationship = DomainObjectBuilder.CreateActiveRelationship();

        _repoMock.Setup(r => r.GetByPatientIdAsync(relationship.PatientId))
            .ReturnsAsync(new[] { relationship });

        _repoMock.Setup(r => r.GetByPsychologistIdAsync(It.IsAny<string>()))
            .ReturnsAsync(Enumerable.Empty<SoftFocusBackend.Therapy.Domain.Model.Aggregates.TherapeuticRelationship>());

        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<SoftFocusBackend.Therapy.Domain.Model.Aggregates.TherapeuticRelationship>()))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.Handle(new TerminateRelationshipCommand
        {
            UserId         = relationship.PatientId,
            RelationshipId = relationship.Id
        });

        // Assert
        relationship.Status.Should().Be(TherapyStatus.Terminated);
        relationship.IsActive.Should().BeFalse();
    }

    // ─── Escenarios de error ──────────────────────────────────────────────

    [Fact]
    public async Task Handle_RelationshipNotFound_ThrowsInvalidOperationException()
    {
        // Arrange — los repos devuelven colecciones vacías
        _repoMock.Setup(r => r.GetByPatientIdAsync(It.IsAny<string>()))
            .ReturnsAsync(Enumerable.Empty<SoftFocusBackend.Therapy.Domain.Model.Aggregates.TherapeuticRelationship>());

        _repoMock.Setup(r => r.GetByPsychologistIdAsync(It.IsAny<string>()))
            .ReturnsAsync(Enumerable.Empty<SoftFocusBackend.Therapy.Domain.Model.Aggregates.TherapeuticRelationship>());

        // Act
        var act = () => _sut.Handle(new TerminateRelationshipCommand
        {
            UserId         = DomainObjectBuilder.PsychologistId,
            RelationshipId = "id-inexistente"
        });

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_UserNotPartOfRelationship_ThrowsUnauthorizedAccessException()
    {
        // Arrange — relación existe pero el userId no es ni psicólogo ni paciente
        var relationship = DomainObjectBuilder.CreateActiveRelationship();
        var impostor     = MongoDB.Bson.ObjectId.GenerateNewId().ToString();

        _repoMock.Setup(r => r.GetByPatientIdAsync(impostor))
            .ReturnsAsync(new[] { relationship }); // devuelve la relación por búsqueda...

        _repoMock.Setup(r => r.GetByPsychologistIdAsync(impostor))
            .ReturnsAsync(Enumerable.Empty<SoftFocusBackend.Therapy.Domain.Model.Aggregates.TherapeuticRelationship>());

        // Act — el Id de relación coincide pero userId no pertenece
        var act = () => _sut.Handle(new TerminateRelationshipCommand
        {
            UserId         = impostor,
            RelationshipId = relationship.Id
        });

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_AlreadyTerminated_ThrowsInvalidOperationException()
    {
        // Arrange
        var relationship = DomainObjectBuilder.CreateTerminatedRelationship();

        _repoMock.Setup(r => r.GetByPsychologistIdAsync(relationship.PsychologistId))
            .ReturnsAsync(new[] { relationship });

        _repoMock.Setup(r => r.GetByPatientIdAsync(It.IsAny<string>()))
            .ReturnsAsync(Enumerable.Empty<SoftFocusBackend.Therapy.Domain.Model.Aggregates.TherapeuticRelationship>());

        // Act — llamar Terminate() en una relación ya terminada lanza excepción en el agregado
        var act = () => _sut.Handle(new TerminateRelationshipCommand
        {
            UserId         = relationship.PsychologistId,
            RelationshipId = relationship.Id
        });

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already terminated*");
    }
}