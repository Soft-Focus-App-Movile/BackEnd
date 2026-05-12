// ════════════════════════════════════════════════════════════════════════════
//  TIPO: PRUEBA UNITARIA
//  Clase bajo prueba: PatientDirectoryQueryService
// ════════════════════════════════════════════════════════════════════════════
using FluentAssertions;
using Moq;
using SoftFocusBackend.Therapy.Application.Internal.OutboundServices;
using SoftFocusBackend.Therapy.Application.Internal.QueryServices;
using SoftFocusBackend.Therapy.Domain.Model.Aggregates;
using SoftFocusBackend.Therapy.Domain.Model.Queries;
using SoftFocusBackend.Therapy.Domain.Model.ValueObjects;
using SoftFocusBackend.Therapy.Domain.Repositories;
using SoftFocusBackend.Users.Domain.Model.Aggregates;
using SoftFocusBackend.Users.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Tests.UnitTests.Therapy;

public class PatientDirectoryQueryServiceTests
{
    private readonly Mock<ITherapeuticRelationshipRepository> _repoMock = new();
    private readonly Mock<IPatientFacade> _facadeMock = new();
    private readonly PatientDirectoryQueryService _sut;

    public PatientDirectoryQueryServiceTests()
    {
        _sut = new PatientDirectoryQueryService(_repoMock.Object, _facadeMock.Object);
    }

    [Fact]
    public async Task Handle_GetPatientDirectoryQuery_ReturnsActivePatients()
    {
        // Arrange
        var relationship = new TherapeuticRelationship("psycho123", ConnectionCode.Create("CODE1234"), "patient123");
        
        _repoMock
            .Setup(r => r.GetByPsychologistIdAsync("psycho123"))
            .ReturnsAsync(new List<TherapeuticRelationship> { relationship });

        var user = new User
        {
            Id = "patient123",
            Email = "pat@test.com",
            PasswordHash = "hash",
            UserType = UserType.General,
            FirstName = "Juan",
            LastName = "Perez",
            FullName = "Juan Perez"
        };
        
        _facadeMock
            .Setup(f => f.FetchPatientById("patient123"))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.Handle(new GetPatientDirectoryQuery { PsychologistId = "psycho123" });

        // Assert
        result.Should().NotBeEmpty();
        result.First().PatientId.Should().Be("patient123");
        result.First().PatientName.Should().Be("Juan Perez"); 
    }
}