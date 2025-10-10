using Microsoft.Extensions.Logging;
using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Model.Commands;
using SoftFocusBackend.Library.Domain.Repositories;
using SoftFocusBackend.Shared.Domain.Repositories;

namespace SoftFocusBackend.Library.Application.Internal.CommandServices;

public class CompletionCommandService : ICompletionCommandService
{
    private readonly IContentAssignmentRepository _assignmentRepository;
    private readonly IContentCompletionRepository _completionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CompletionCommandService> _logger;

    public CompletionCommandService(
        IContentAssignmentRepository assignmentRepository,
        IContentCompletionRepository completionRepository,
        IUnitOfWork unitOfWork,
        ILogger<CompletionCommandService> logger)
    {
        _assignmentRepository = assignmentRepository;
        _completionRepository = completionRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task MarkAsCompletedAsync(MarkAsCompletedCommand command)
    {
        command.Validate();

        // Buscar la asignación y verificar que pertenezca al paciente
        var assignment = await _assignmentRepository.FindByIdAndPatientAsync(
            command.AssignmentId, command.PatientId);

        if (assignment == null)
        {
            throw new InvalidOperationException("Asignación no encontrada o no pertenece a este usuario");
        }

        if (assignment.IsCompleted)
        {
            throw new InvalidOperationException("La asignación ya está completada");
        }

        // Marcar como completada
        assignment.MarkAsCompleted();
        _assignmentRepository.Update(assignment);

        // Crear registro de finalización
        var completion = ContentCompletion.Create(command.AssignmentId, command.PatientId);
        await _completionRepository.AddAsync(completion);

        await _unitOfWork.CompleteAsync();

        _logger.LogInformation(
            "Assignment {AssignmentId} marked as completed by patient: {PatientId}",
            command.AssignmentId, command.PatientId);
    }
}
