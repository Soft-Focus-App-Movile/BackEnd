// ════════════════════════════════════════════════════════════════════════════
//  TIPO: PRUEBA UNITARIA
//  Clase bajo prueba: CompletionCommandService
// ════════════════════════════════════════════════════════════════════════════
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SoftFocusBackend.Library.Application.Internal.CommandServices;
using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Model.Commands;
using SoftFocusBackend.Library.Domain.Model.ValueObjects;
using SoftFocusBackend.Library.Domain.Repositories;
using SoftFocusBackend.Shared.Domain.Repositories;
using SoftFocusBackend.Shared.Infrastructure.Events;

namespace SoftFocusBackend.Tests.UnitTests.Library;

public class CompletionCommandServiceTests
{
    // ─── colaboradores mockeados ──────────────────────────────────────────
    private readonly Mock<IContentAssignmentRepository>        _assignRepoMock     = new();
    private readonly Mock<IContentCompletionRepository>        _completionRepoMock = new();
    private readonly Mock<IUnitOfWork>                         _unitOfWorkMock     = new();
    private readonly Mock<IDomainEventBus>                     _eventBusMock       = new();
    private readonly Mock<ILogger<CompletionCommandService>>   _loggerMock         = new();

    private readonly CompletionCommandService _sut;

    public CompletionCommandServiceTests()
    {
        _sut = new CompletionCommandService(
            _assignRepoMock.Object,
            _completionRepoMock.Object,
            _unitOfWorkMock.Object,
            _eventBusMock.Object,
            _loggerMock.Object);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────

    private static ContentItem BuildContentItem()
    {
        var metadata = new ContentMetadata { Title = "Video de Meditación" };
        return ContentItem.Create("yt-video-001", ContentType.Video, metadata, new List<EmotionalTag> { EmotionalTag.Calm }, "https://youtube.com", 24);
    }

    private static ContentAssignment BuildPendingAssignment(
        string psychologistId = "psych-001",
        string patientId      = "patient-001")
    {
        var content = BuildContentItem();
        return ContentAssignment.Create(psychologistId, patientId, content, "Ver este video esta semana");
    }

    private static ContentAssignment BuildCompletedAssignment()
    {
        var assignment = BuildPendingAssignment();
        assignment.MarkAsCompleted();
        return assignment;
    }

    private static MarkAsCompletedCommand BuildCommand(
        string patientId    = "patient-001",
        string assignmentId = "assign-001") =>
        new(patientId, assignmentId);

    // ─── MarkAsCompletedAsync — Escenarios felices ────────────────────────

    [Fact]
    public async Task MarkAsCompletedAsync_ValidAssignment_UpdatesAssignmentInRepository()
    {
        // Arrange
        var assignment = BuildPendingAssignment();
        var command    = BuildCommand();

        _assignRepoMock.Setup(r => r.FindByIdAndPatientAsync("assign-001", "patient-001")).ReturnsAsync(assignment);
        _assignRepoMock.Setup(r => r.Update(assignment));
        _completionRepoMock.Setup(r => r.AddAsync(It.IsAny<ContentCompletion>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);
        _assignRepoMock.Setup(r => r.FindPendingByPatientIdAsync("patient-001"))
            .ReturnsAsync(new List<ContentAssignment>());
        _assignRepoMock.Setup(r => r.FindCompletedByPatientIdAsync("patient-001"))
            .ReturnsAsync(new List<ContentAssignment> { assignment });

        // Act
        await _sut.MarkAsCompletedAsync(command);

        // Assert
        _assignRepoMock.Verify(r => r.Update(assignment), Times.Once);
    }

    [Fact]
    public async Task MarkAsCompletedAsync_ValidAssignment_CreatesCompletionRecord()
    {
        // Arrange
        var assignment = BuildPendingAssignment();
        var command    = BuildCommand();

        _assignRepoMock.Setup(r => r.FindByIdAndPatientAsync("assign-001", "patient-001")).ReturnsAsync(assignment);
        _assignRepoMock.Setup(r => r.Update(assignment));
        _completionRepoMock.Setup(r => r.AddAsync(It.IsAny<ContentCompletion>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);
        _assignRepoMock.Setup(r => r.FindPendingByPatientIdAsync("patient-001"))
            .ReturnsAsync(new List<ContentAssignment>());
        _assignRepoMock.Setup(r => r.FindCompletedByPatientIdAsync("patient-001"))
            .ReturnsAsync(new List<ContentAssignment> { assignment });

        // Act
        await _sut.MarkAsCompletedAsync(command);

        // Assert
        _completionRepoMock.Verify(r => r.AddAsync(It.IsAny<ContentCompletion>()), Times.Once);
    }

    [Fact]
    public async Task MarkAsCompletedAsync_ValidAssignment_CompletesUnitOfWork()
    {
        // Arrange
        var assignment = BuildPendingAssignment();
        var command    = BuildCommand();

        _assignRepoMock.Setup(r => r.FindByIdAndPatientAsync("assign-001", "patient-001")).ReturnsAsync(assignment);
        _completionRepoMock.Setup(r => r.AddAsync(It.IsAny<ContentCompletion>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CompleteAsync()).Returns(Task.CompletedTask);
        _assignRepoMock.Setup(r => r.FindPendingByPatientIdAsync("patient-001"))
            .ReturnsAsync(new List<ContentAssignment>());
        _assignRepoMock.Setup(r => r.FindCompletedByPatientIdAsync("patient-001"))
            .ReturnsAsync(new List<ContentAssignment>());

        // Act
        await _sut.MarkAsCompletedAsync(command);

        // Assert
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    // ─── MarkAsCompletedAsync — Escenarios de error ───────────────────────

    [Fact]
    public async Task MarkAsCompletedAsync_AssignmentNotFoundOrWrongPatient_ThrowsInvalidOperationException()
    {
        // Arrange
        var command = BuildCommand("patient-001", "nonexistent-assign");

        _assignRepoMock
            .Setup(r => r.FindByIdAndPatientAsync("nonexistent-assign", "patient-001"))
            .ReturnsAsync((ContentAssignment?)null);

        // Act
        var act = async () => await _sut.MarkAsCompletedAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*no encontrada*");
        _completionRepoMock.Verify(r => r.AddAsync(It.IsAny<ContentCompletion>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }

    [Fact]
    public async Task MarkAsCompletedAsync_AlreadyCompleted_ThrowsInvalidOperationException()
    {
        // Arrange — la asignación ya está completada
        var completedAssignment = BuildCompletedAssignment();
        var command             = BuildCommand();

        _assignRepoMock
            .Setup(r => r.FindByIdAndPatientAsync("assign-001", "patient-001"))
            .ReturnsAsync(completedAssignment);

        // Act
        var act = async () => await _sut.MarkAsCompletedAsync(command);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ya está completada*");
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }
}
