// ════════════════════════════════════════════════════════════════════════════
//  TIPO: PRUEBA UNITARIA
//  Clase bajo prueba: ConnectionValidationService
// ════════════════════════════════════════════════════════════════════════════
using FluentAssertions;
using Moq;
using SoftFocusBackend.Therapy.Domain.Model.Aggregates;
using SoftFocusBackend.Therapy.Domain.Model.ValueObjects;
using SoftFocusBackend.Therapy.Domain.Repositories;
using SoftFocusBackend.Therapy.Domain.Services;
using SoftFocusBackend.Users.Domain.Model.Aggregates;
using SoftFocusBackend.Users.Domain.Model.ValueObjects;
using SoftFocusBackend.Users.Infrastructure.Persistence.MongoDB.Repositories;

namespace SoftFocusBackend.Tests.UnitTests.Therapy;

public class ConnectionValidationServiceTests
{
    private readonly Mock<ITherapeuticRelationshipRepository> _relationshipRepoMock = new();
    private readonly Mock<IPsychologistRepository> _psychologistRepoMock = new();
    private readonly ConnectionValidationService _sut;

    public ConnectionValidationServiceTests()
    {
        _sut = new ConnectionValidationService(_relationshipRepoMock.Object, _psychologistRepoMock.Object);
    }

    [Fact]
    public async Task EstablishConnectionAsync_WithValidCode_CreatesRelationship()
    {
        // Arrange
        var connectionCode = ConnectionCode.Create("CODE1234");
        
        var psychologist = new PsychologistUser
        {
            Id = "psy1",
            Email = "email@t.com",
            FirstName = "Dr.",
            LastName = "House",
            LicenseNumber = "Licencia123",
            Specialties = new List<PsychologySpecialty> { PsychologySpecialty.Clinica }
        };
        
        psychologist.GenerateNewInvitationCode(); 
        psychologist.InvitationCode = connectionCode.Value;
        
        _psychologistRepoMock
            .Setup(r => r.FindByInvitationCodeAsync(connectionCode.Value))
            .ReturnsAsync(psychologist);
            
        _relationshipRepoMock
            .Setup(r => r.GetByPatientIdAsync("patient123"))
            .ReturnsAsync(new List<TherapeuticRelationship>());

        // Act
        var result = await _sut.EstablishConnectionAsync("patient123", connectionCode);

        // Assert
        result.Should().NotBeNull();
        result.PatientId.Should().Be("patient123");
        result.PsychologistId.Should().Be(psychologist.Id);
    }
}