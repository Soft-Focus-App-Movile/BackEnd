// ════════════════════════════════════════════════════════════════════════════
//  TIPO: PRUEBA UNITARIA
//  Clase bajo prueba: CrisisAlertCommandService
// ════════════════════════════════════════════════════════════════════════════
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SoftFocusBackend.Crisis.Application.Internal.CommandServices;
using SoftFocusBackend.Crisis.Domain.Model.Aggregates;
using SoftFocusBackend.Crisis.Domain.Model.Commands;
using SoftFocusBackend.Crisis.Domain.Model.Events;
using SoftFocusBackend.Crisis.Domain.Model.ValueObjects;
using SoftFocusBackend.Crisis.Domain.Repositories;
using SoftFocusBackend.Crisis.Domain.Services;
using SoftFocusBackend.Shared.Domain.Repositories;
using SoftFocusBackend.Shared.Infrastructure.Events;
using SoftFocusBackend.Therapy.Domain.Model.Aggregates;
using SoftFocusBackend.Therapy.Domain.Repositories;
using SoftFocusBackend.Tests.Helpers;

namespace SoftFocusBackend.Tests.UnitTests.Crisis;

public class CrisisAlertCommandServiceTests
{
    private readonly Mock<ICrisisAlertRepository> _crisisAlertRepoMock = new();
    private readonly Mock<ITherapeuticRelationshipRepository> _therapyRepoMock = new();
    private readonly Mock<ICrisisNotificationService> _notificationMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IDomainEventBus> _eventBusMock = new();
    private readonly Mock<ILogger<CrisisAlertCommandService>> _loggerMock = new();

    private readonly CrisisAlertCommandService _sut;

    public CrisisAlertCommandServiceTests()
    {
        _sut = new CrisisAlertCommandService(
            _crisisAlertRepoMock.Object,
            _therapyRepoMock.Object,
            _notificationMock.Object,
            _uowMock.Object,
            _eventBusMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_CreateCrisisAlertCommand_Success()
    {
        // Arrange
        var patientId = DomainObjectBuilder.PatientId;
        var psychologistId = DomainObjectBuilder.PsychologistId;
        
        var command = new CreateCrisisAlertCommand(patientId, AlertSeverity.Critical, "APP_BUTTON", "Feeling overwhelmed");
        
        // Creamos una relación activa usando el Helper
        var activeRelationship = DomainObjectBuilder.CreateActiveRelationship(psychologistId, patientId);
        
        _therapyRepoMock
            .Setup(r => r.GetByPatientIdAsync(patientId))
            .ReturnsAsync(new List<TherapeuticRelationship> { activeRelationship });

        _crisisAlertRepoMock.Setup(r => r.AddAsync(It.IsAny<CrisisAlert>())).Returns(Task.CompletedTask);
        _uowMock.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);
        // Act
        var result = await _sut.Handle(command);

        // Assert
        result.Should().NotBeNull();
        result.PatientId.Should().Be(patientId);
        result.Severity.Should().Be(AlertSeverity.Critical);

        _uowMock.Verify(u => u.CompleteAsync(), Times.Once);
        _notificationMock.Verify(n => n.NotifyPsychologistAsync(It.IsAny<CrisisAlert>()), Times.Once);
        _eventBusMock.Verify(e => e.PublishAsync(It.IsAny<CrisisAlertCreatedEvent>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UpdateAlertStatusCommand_UpdatesAndPersists()
    {
        // Arrange
        var alert = DomainObjectBuilder.CreateCrisisAlert();
        var command = new UpdateAlertStatusCommand(alert.Id, AlertStatus.Resolved, "Todo controlado");

        _crisisAlertRepoMock.Setup(r => r.FindByIdAsync(alert.Id)).ReturnsAsync(alert);

        // Act
        var result = await _sut.Handle(command);

        // Assert
        result.Status.Should().Be(AlertStatus.Resolved);
        result.PsychologistNotes.Should().Be("Todo controlado");
        
        _crisisAlertRepoMock.Verify(r => r.Update(alert), Times.Once);
        _uowMock.Verify(u => u.CompleteAsync(), Times.Once);
    }
}