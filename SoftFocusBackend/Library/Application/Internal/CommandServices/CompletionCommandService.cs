
using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Model.Commands;
using SoftFocusBackend.Library.Domain.Model.Events;
using SoftFocusBackend.Library.Domain.Repositories;
using SoftFocusBackend.Shared.Domain.Repositories;
using SoftFocusBackend.Shared.Infrastructure.Events;

namespace SoftFocusBackend.Library.Application.Internal.CommandServices;

public class CompletionCommandService : ICompletionCommandService
{
    private readonly IContentAssignmentRepository _assignmentRepository;
    private readonly IContentCompletionRepository _completionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventBus _eventBus; // ← NUEVO
    private readonly ILogger<CompletionCommandService> _logger;

    public CompletionCommandService(
        IContentAssignmentRepository assignmentRepository,
        IContentCompletionRepository completionRepository,
        IUnitOfWork unitOfWork,
        IDomainEventBus eventBus,
        ILogger<CompletionCommandService> logger)
    {
        _assignmentRepository = assignmentRepository;
        _completionRepository = completionRepository;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
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

        // 🔥 NUEVO: Publicar evento de asignación completada
        try
        {
            var completedEvent = new AssignmentCompletedEvent(
                assignmentId: assignment.Id,
                patientId: command.PatientId,
                psychologistId: assignment.PsychologistId,
                contentTitle: assignment.Content.Metadata?.Title ?? "Sin título",
                contentType: assignment.ContentType.ToString()
            );

            await _eventBus.PublishAsync(completedEvent);

            _logger.LogInformation(
                "AssignmentCompletedEvent published for assignment {AssignmentId}",
                assignment.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error publishing AssignmentCompletedEvent for assignment {AssignmentId}: {Error}",
                assignment.Id, ex.Message);
        }

        // 🔥 NUEVO: Verificar si completó TODAS las asignaciones
        try
        {
            var pendingAssignments = await _assignmentRepository.FindPendingByPatientIdAsync(command.PatientId);
            
            if (!pendingAssignments.Any())
            {
                var completedAssignments = await _assignmentRepository.FindCompletedByPatientIdAsync(command.PatientId);
                
                var allCompletedEvent = new AllAssignmentsCompletedEvent(
                    patientId: command.PatientId,
                    psychologistId: assignment.PsychologistId,
                    completedCount: completedAssignments.Count()
                );

                await _eventBus.PublishAsync(allCompletedEvent);

                _logger.LogInformation(
                    "AllAssignmentsCompletedEvent published for patient {PatientId}",
                    command.PatientId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error checking/publishing AllAssignmentsCompletedEvent: {Error}",
                ex.Message);
        }
    }
}
