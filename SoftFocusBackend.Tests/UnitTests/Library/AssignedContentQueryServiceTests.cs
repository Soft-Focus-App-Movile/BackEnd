// ════════════════════════════════════════════════════════════════════════════
//  TIPO: PRUEBA UNITARIA
//  Clase bajo prueba: AssignedContentQueryService
// ════════════════════════════════════════════════════════════════════════════
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SoftFocusBackend.Library.Application.Internal.QueryServices;
using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Model.Queries;
using SoftFocusBackend.Library.Domain.Model.ValueObjects;
using SoftFocusBackend.Library.Domain.Repositories;

namespace SoftFocusBackend.Tests.UnitTests.Library;

public class AssignedContentQueryServiceTests
{
    // ─── colaboradores mockeados ──────────────────────────────────────────
    private readonly Mock<IContentAssignmentRepository>         _assignRepoMock = new();
    private readonly Mock<ILogger<AssignedContentQueryService>> _loggerMock     = new();

    private readonly AssignedContentQueryService _sut;

    public AssignedContentQueryServiceTests()
    {
        _sut = new AssignedContentQueryService(
            _assignRepoMock.Object,
            _loggerMock.Object);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────

    private static ContentAssignment BuildAssignment(
        string psychologistId = "psych-001",
        string patientId      = "patient-001",
        bool completed        = false)
    {
        var metadata   = new ContentMetadata { Title = "Contenido Asignado" };
        var content    = ContentItem.Create("ext-001", ContentType.Music, metadata, new List<EmotionalTag>(), "https://spotify.com", 24);
        var assignment = ContentAssignment.Create(psychologistId, patientId, content, "Nota terapéutica");
        if (completed)
            assignment.MarkAsCompleted();
        return assignment;
    }

    // ─── GetAssignedContentAsync — Escenarios ────────────────────────────

    [Fact]
    public async Task GetAssignedContentAsync_NoFilter_ReturnsAllAssignments()
    {
        // Arrange
        var assignments = new List<ContentAssignment>
        {
            BuildAssignment(completed: false),
            BuildAssignment(completed: false)
        };
        var query = new GetAssignedContentQuery("patient-001");

        _assignRepoMock
            .Setup(r => r.FindByPatientIdAsync("patient-001"))
            .ReturnsAsync(assignments);

        // Act
        var result = await _sut.GetAssignedContentAsync(query);

        // Assert
        result.Should().HaveCount(2);
        _assignRepoMock.Verify(r => r.FindByPatientIdAsync("patient-001"), Times.Once);
    }

    [Fact]
    public async Task GetAssignedContentAsync_PendingFilter_ReturnsPendingOnly()
    {
        // Arrange
        var pendingAssignments = new List<ContentAssignment> { BuildAssignment(completed: false) };
        var query              = new GetAssignedContentQuery("patient-001", completedFilter: false);

        _assignRepoMock
            .Setup(r => r.FindPendingByPatientIdAsync("patient-001"))
            .ReturnsAsync(pendingAssignments);

        // Act
        var result = await _sut.GetAssignedContentAsync(query);

        // Assert
        result.Should().HaveCount(1);
        _assignRepoMock.Verify(r => r.FindPendingByPatientIdAsync("patient-001"), Times.Once);
        _assignRepoMock.Verify(r => r.FindByPatientIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetAssignedContentAsync_CompletedFilter_ReturnsCompletedOnly()
    {
        // Arrange
        var completedAssignments = new List<ContentAssignment> { BuildAssignment(completed: true) };
        var query                = new GetAssignedContentQuery("patient-001", completedFilter: true);

        _assignRepoMock
            .Setup(r => r.FindCompletedByPatientIdAsync("patient-001"))
            .ReturnsAsync(completedAssignments);

        // Act
        var result = await _sut.GetAssignedContentAsync(query);

        // Assert
        result.Should().HaveCount(1);
        _assignRepoMock.Verify(r => r.FindCompletedByPatientIdAsync("patient-001"), Times.Once);
    }

    [Fact]
    public async Task GetAssignedContentAsync_EmptyPatientId_ThrowsArgumentException()
    {
        // Arrange
        var query = new GetAssignedContentQuery(string.Empty);

        // Act
        var act = async () => await _sut.GetAssignedContentAsync(query);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    // ─── GetAssignmentsByPsychologistAsync — Escenarios ───────────────────

    [Fact]
    public async Task GetAssignmentsByPsychologistAsync_WithoutPatient_ReturnsAllForPsychologist()
    {
        // Arrange
        var assignments = new List<ContentAssignment>
        {
            BuildAssignment("psych-001", "patient-001"),
            BuildAssignment("psych-001", "patient-002")
        };

        _assignRepoMock
            .Setup(r => r.FindByPsychologistIdAsync("psych-001"))
            .ReturnsAsync(assignments);

        // Act
        var result = await _sut.GetAssignmentsByPsychologistAsync("psych-001");

        // Assert
        result.Should().HaveCount(2);
        _assignRepoMock.Verify(r => r.FindByPsychologistIdAsync("psych-001"), Times.Once);
        _assignRepoMock.Verify(r => r.FindByPsychologistAndPatientAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetAssignmentsByPsychologistAsync_WithPatient_ReturnsFilteredByPatient()
    {
        // Arrange
        var filtered = new List<ContentAssignment> { BuildAssignment("psych-001", "patient-001") };

        _assignRepoMock
            .Setup(r => r.FindByPsychologistAndPatientAsync("psych-001", "patient-001"))
            .ReturnsAsync(filtered);

        // Act
        var result = await _sut.GetAssignmentsByPsychologistAsync("psych-001", "patient-001");

        // Assert
        result.Should().HaveCount(1);
        _assignRepoMock.Verify(r => r.FindByPsychologistAndPatientAsync("psych-001", "patient-001"), Times.Once);
        _assignRepoMock.Verify(r => r.FindByPsychologistIdAsync(It.IsAny<string>()), Times.Never);
    }
}
