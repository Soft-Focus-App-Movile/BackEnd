// ════════════════════════════════════════════════════════════════════════════
//  TIPO: PRUEBA UNITARIA
//  Clase bajo prueba: CrisisAlertQueryService
// ════════════════════════════════════════════════════════════════════════════
using FluentAssertions;
using Moq;
using SoftFocusBackend.Crisis.Application.Internal.QueryServices;
using SoftFocusBackend.Crisis.Domain.Model.Aggregates;
using SoftFocusBackend.Crisis.Domain.Model.Queries;
using SoftFocusBackend.Crisis.Domain.Repositories;
using SoftFocusBackend.Tests.Helpers;

namespace SoftFocusBackend.Tests.UnitTests.Crisis;

public class CrisisAlertQueryServiceTests
{
    private readonly Mock<ICrisisAlertRepository> _repoMock = new();
    private readonly CrisisAlertQueryService _sut;

    public CrisisAlertQueryServiceTests()
    {
        _sut = new CrisisAlertQueryService(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_GetAlertByIdQuery_ReturnsAlert()
    {
        // Arrange
        var alert = DomainObjectBuilder.CreateCrisisAlert();
        var query = new GetAlertByIdQuery(alert.Id);

        _repoMock.Setup(r => r.FindByIdAsync(alert.Id)).ReturnsAsync(alert);

        // Act
        var result = await _sut.Handle(query);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(alert.Id);
    }

    [Fact]
    public async Task Handle_GetPsychologistAlertsQuery_ReturnsAlerts()
    {
        // Arrange
        var psychologistId = DomainObjectBuilder.PsychologistId;
        var alert = DomainObjectBuilder.CreateCrisisAlert(psychologistId: psychologistId);
        
        var query = new GetPsychologistAlertsQuery(psychologistId, null, null, 10);
        
        _repoMock
            .Setup(r => r.FindByPsychologistIdAsync(psychologistId, null, null, 10))
            .ReturnsAsync(new List<CrisisAlert> { alert });

        // Act
        var result = await _sut.Handle(query);

        // Assert
        result.Should().NotBeEmpty();
        result.First().PsychologistId.Should().Be(psychologistId);
    }

    [Fact]
    public async Task GetPendingAlertCount_ReturnsCount()
    {
        // Arrange
        var psychologistId = DomainObjectBuilder.PsychologistId;
        _repoMock
            .Setup(r => r.CountPendingAlertsByPsychologistAsync(psychologistId))
            .ReturnsAsync(5);

        // Act
        var result = await _sut.GetPendingAlertCount(psychologistId);

        // Assert
        result.Should().Be(5);
    }
}