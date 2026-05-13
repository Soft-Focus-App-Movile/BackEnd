// ════════════════════════════════════════════════════════════════════════════
//  TIPO: PRUEBA UNITARIA
//  Clase bajo prueba: AssignmentCommandService
// ════════════════════════════════════════════════════════════════════════════
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SoftFocusBackend.Library.Application.ACL.Services;
using SoftFocusBackend.Library.Application.Internal.CommandServices;
using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Model.Commands;
using SoftFocusBackend.Library.Domain.Model.ValueObjects;
using SoftFocusBackend.Library.Domain.Repositories;
using SoftFocusBackend.Shared.Domain.Repositories;
using SoftFocusBackend.Shared.Infrastructure.Events;
using SoftFocusBackend.Users.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Tests.UnitTests.Library;

public class AssignmentCommandServiceTests
{
    // ─── colaboradores mockeados ──────────────────────────────────────────
    private readonly Mock<IContentAssignmentRepository>        _assignRepoMock  = new();
    private readonly Mock<IContentItemRepository>              _contentRepoMock = new();
    private readonly Mock<IUserIntegrationService>             _userIntMock     = new();
    private readonly Mock<IUnitOfWork>                         _unitOfWorkMock  = new();
    private readonly Mock<IDomainEventBus>                     _eventBusMock    = new();
    private readonly Mock<ILogger<AssignmentCommandService>>   _loggerMock      = new();

    private readonly AssignmentCommandService _sut;

    public AssignmentCommandServiceTests()
    {
        _sut = new AssignmentCommandService(
            _assignRepoMock.Object,
            _contentRepoMock.Object,
            _userIntMock.Object,
            _unitOfWorkMock.Object,
            _eventBusMock.Object,
            _loggerMock.Object);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────

    private static ContentItem BuildContentItem(string externalId = "tmdb-movie-001")
    {
        var metadata = new ContentMetadata { Title = "Película Terapéutica" };
        return ContentItem.Create(externalId, ContentType.Movie, metadata, new List<EmotionalTag> { EmotionalTag.Calm }, "https://tmdb.com", 24);
    }

    private static AssignContentCommand BuildCommand(
        string psychologistId    = "psych-001",
        List<string>? patientIds = null,
        string contentId         = "tmdb-movie-001",
        string notes             = "Ver esta película esta semana") =>
        new(psychologistId, patientIds ?? new List<string> { "patient-001" }, contentId, ContentType.Movie, notes);

    // ─── AssignContentAsync — Escenarios felices ──────────────────────────

    [Fact]
    public async Task AssignContentAsync_Psychologist_SinglePatient_ReturnsOneAssignmentId()
    {
        // Arrange
        var command = BuildCommand();
        var content = BuildContentItem();

        _userIntMock.Setup(u => u.GetUserTypeAsync("psych-001")).ReturnsAsync(UserType.Psychologist);
        _userIntMock.Setup(u => u.ValidateUserExistsAsync("patient-001")).ReturnsAsync(true);
        _userIntMock.Setup(u => u.ValidatePatientBelongsToPsychologistAsync("patient-001", "psych-001")).ReturnsAsync(true);
        _contentRepoMock.Setup(r => r.FindByExternalIdAsync("tmdb-movie-001")).ReturnsAsync(content);
        _assignRepoMock.Setup(r => r.AddAsync(It.IsAny<ContentAssignment>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.AssignContentAsync(command);

        // Assert
        result.Should().HaveCount(1);
        result.First().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task AssignContentAsync_Psychologist_MultiplePatients_ReturnsAllIds()
    {
        // Arrange
        var command = BuildCommand(patientIds: new List<string> { "patient-001", "patient-002" });
        var content = BuildContentItem();

        _userIntMock.Setup(u => u.GetUserTypeAsync("psych-001")).ReturnsAsync(UserType.Psychologist);
        _userIntMock.Setup(u => u.ValidateUserExistsAsync(It.IsAny<string>())).ReturnsAsync(true);
        _userIntMock.Setup(u => u.ValidatePatientBelongsToPsychologistAsync(It.IsAny<string>(), "psych-001")).ReturnsAsync(true);
        _contentRepoMock.Setup(r => r.FindByExternalIdAsync("tmdb-movie-001")).ReturnsAsync(content);
        _assignRepoMock.Setup(r => r.AddAsync(It.IsAny<ContentAssignment>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _sut.AssignContentAsync(command);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task AssignContentAsync_ValidPsychologist_CallsAddAsyncForEachPatient()
    {
        // Arrange
        var command = BuildCommand(patientIds: new List<string> { "patient-001", "patient-002" });
        var content = BuildContentItem();

        _userIntMock.Setup(u => u.GetUserTypeAsync("psych-001")).ReturnsAsync(UserType.Psychologist);
        _userIntMock.Setup(u => u.ValidateUserExistsAsync(It.IsAny<string>())).ReturnsAsync(true);
        _userIntMock.Setup(u => u.ValidatePatientBelongsToPsychologistAsync(It.IsAny<string>(), "psych-001")).ReturnsAsync(true);
        _contentRepoMock.Setup(r => r.FindByExternalIdAsync("tmdb-movie-001")).ReturnsAsync(content);
        _assignRepoMock.Setup(r => r.AddAsync(It.IsAny<ContentAssignment>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);

        // Act
        await _sut.AssignContentAsync(command);

        // Assert
        _assignRepoMock.Verify(r => r.AddAsync(It.IsAny<ContentAssignment>()), Times.Exactly(2));
    }

    [Fact]
    public async Task AssignContentAsync_ValidPsychologist_CompletesUnitOfWorkOnce()
    {
        // Arrange
        var command = BuildCommand();
        var content = BuildContentItem();

        _userIntMock.Setup(u => u.GetUserTypeAsync("psych-001")).ReturnsAsync(UserType.Psychologist);
        _userIntMock.Setup(u => u.ValidateUserExistsAsync("patient-001")).ReturnsAsync(true);
        _userIntMock.Setup(u => u.ValidatePatientBelongsToPsychologistAsync("patient-001", "psych-001")).ReturnsAsync(true);
        _contentRepoMock.Setup(r => r.FindByExternalIdAsync("tmdb-movie-001")).ReturnsAsync(content);
        _assignRepoMock.Setup(r => r.AddAsync(It.IsAny<ContentAssignment>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);

        // Act
        await _sut.AssignContentAsync(command);

        // Assert
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    // ─── AssignContentAsync — Escenarios de error ────────────────────────

    [Fact]
    public async Task AssignContentAsync_NonPsychologist_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var command = BuildCommand();

        _userIntMock.Setup(u => u.GetUserTypeAsync("psych-001")).ReturnsAsync(UserType.General);

        // Act
        var act = async () => await _sut.AssignContentAsync(command);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*psicólogos*");
        _assignRepoMock.Verify(r => r.AddAsync(It.IsAny<ContentAssignment>()), Times.Never);
    }

    [Fact]
    public async Task AssignContentAsync_PatientDoesNotExist_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var command = BuildCommand();

        _userIntMock.Setup(u => u.GetUserTypeAsync("psych-001")).ReturnsAsync(UserType.Psychologist);
        _userIntMock.Setup(u => u.ValidateUserExistsAsync("patient-001")).ReturnsAsync(false);

        // Act
        var act = async () => await _sut.AssignContentAsync(command);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _assignRepoMock.Verify(r => r.AddAsync(It.IsAny<ContentAssignment>()), Times.Never);
    }

    [Fact]
    public async Task AssignContentAsync_PatientNotRelatedToPsychologist_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var command = BuildCommand();

        _userIntMock.Setup(u => u.GetUserTypeAsync("psych-001")).ReturnsAsync(UserType.Psychologist);
        _userIntMock.Setup(u => u.ValidateUserExistsAsync("patient-001")).ReturnsAsync(true);
        _userIntMock.Setup(u => u.ValidatePatientBelongsToPsychologistAsync("patient-001", "psych-001")).ReturnsAsync(false);

        // Act
        var act = async () => await _sut.AssignContentAsync(command);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*relación terapéutica*");
        _assignRepoMock.Verify(r => r.AddAsync(It.IsAny<ContentAssignment>()), Times.Never);
    }

    [Fact]
    public async Task AssignContentAsync_ContentNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = BuildCommand();

        _userIntMock.Setup(u => u.GetUserTypeAsync("psych-001")).ReturnsAsync(UserType.Psychologist);
        _userIntMock.Setup(u => u.ValidateUserExistsAsync("patient-001")).ReturnsAsync(true);
        _userIntMock.Setup(u => u.ValidatePatientBelongsToPsychologistAsync("patient-001", "psych-001")).ReturnsAsync(true);
        _contentRepoMock.Setup(r => r.FindByExternalIdAsync("tmdb-movie-001")).ReturnsAsync((ContentItem?)null);

        // Act
        var act = async () => await _sut.AssignContentAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Contenido no encontrado*");
        _assignRepoMock.Verify(r => r.AddAsync(It.IsAny<ContentAssignment>()), Times.Never);
    }
}
