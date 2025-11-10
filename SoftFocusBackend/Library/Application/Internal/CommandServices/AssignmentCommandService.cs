using SoftFocusBackend.Library.Application.ACL.Services;
using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Model.Commands;
using SoftFocusBackend.Library.Domain.Model.Events;
using SoftFocusBackend.Library.Domain.Repositories;
using SoftFocusBackend.Shared.Domain.Repositories;
using SoftFocusBackend.Shared.Infrastructure.Events;
using SoftFocusBackend.Users.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Library.Application.Internal.CommandServices;

public class AssignmentCommandService : IAssignmentCommandService
{
    private readonly IContentAssignmentRepository _assignmentRepository;
    private readonly IContentItemRepository _contentRepository;
    private readonly IUserIntegrationService _userIntegration;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventBus _eventBus; // ← NUEVO
    private readonly ILogger<AssignmentCommandService> _logger;

    public AssignmentCommandService(
        IContentAssignmentRepository assignmentRepository,
        IContentItemRepository contentRepository,
        IUserIntegrationService userIntegration,
        IUnitOfWork unitOfWork,
        IDomainEventBus eventBus,
        ILogger<AssignmentCommandService> logger)
    {
        _assignmentRepository = assignmentRepository;
        _contentRepository = contentRepository;
        _userIntegration = userIntegration;
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task<List<string>> AssignContentAsync(AssignContentCommand command)
    {
        command.Validate();

        // Validar que el usuario sea Psicólogo
        var userType = await _userIntegration.GetUserTypeAsync(command.PsychologistId);
        if (userType != UserType.Psychologist)
        {
            throw new UnauthorizedAccessException("Solo psicólogos pueden asignar contenido");
        }

        // ✅ MEJORA: Validar TODOS los pacientes PRIMERO antes de crear asignaciones
        var validationErrors = new List<string>();

        foreach (var patientId in command.PatientIds)
        {
            var patientExists = await _userIntegration.ValidateUserExistsAsync(patientId);
            if (!patientExists)
            {
                validationErrors.Add($"Usuario no encontrado: {patientId}");
                continue; // Continuar validando los demás
            }

            var belongsToPsychologist = await _userIntegration.ValidatePatientBelongsToPsychologistAsync(
                patientId, command.PsychologistId);

            if (!belongsToPsychologist)
            {
                validationErrors.Add(
                    $"El usuario {patientId} no tiene una relación terapéutica activa con este psicólogo");
            }
        }

        // Si hay errores de validación, fallar ANTES de crear cualquier asignación
        if (validationErrors.Any())
        {
            var errorMessage = $"No se puede asignar contenido debido a los siguientes errores:\n" +
                              string.Join("\n", validationErrors);
            _logger.LogWarning(
                "Assignment validation failed for psychologist {PsychologistId}: {Errors}",
                command.PsychologistId, string.Join("; ", validationErrors));
            throw new UnauthorizedAccessException(errorMessage);
        }

        // Obtener el contenido
        var content = await _contentRepository.FindByExternalIdAsync(command.ContentId);
        if (content == null)
        {
            throw new InvalidOperationException("Contenido no encontrado");
        }

        // ✅ TRANSACCIÓN ATÓMICA: Crear todas las asignaciones
        // Si falla alguna, el UnitOfWork hará rollback de todas
        var assignmentIds = new List<string>();

        try
        {
            foreach (var patientId in command.PatientIds)
            {
                var assignment = ContentAssignment.Create(
                    command.PsychologistId,
                    patientId,
                    content,
                    command.Notes
                );

                await _assignmentRepository.AddAsync(assignment);
                assignmentIds.Add(assignment.Id);

                _logger.LogDebug(
                    "Assignment created: {AssignmentId} for patient {PatientId}",
                    assignment.Id, patientId);
            }

            // Persistir TODAS las asignaciones de forma atómica
            await _unitOfWork.CompleteAsync();

            // Solo publicar eventos DESPUÉS de que la transacción haya tenido éxito
            foreach (var assignmentId in assignmentIds)
            {
                try
                {
                    var patientId = command.PatientIds[assignmentIds.IndexOf(assignmentId)];
                    var assignmentEvent = new ContentAssignedEvent(
                        assignmentId: assignmentId,
                        psychologistId: command.PsychologistId,
                        patientId: patientId,
                        contentId: content.ExternalId,
                        contentType: content.ContentType.ToString(),
                        contentTitle: content.Metadata?.Title ?? "Sin título",
                        notes: command.Notes
                    );

                    await _eventBus.PublishAsync(assignmentEvent);

                    _logger.LogInformation(
                        "ContentAssignedEvent published for assignment {AssignmentId}",
                        assignmentId);
                }
                catch (Exception ex)
                {
                    // Error en eventos no debe fallar la operación (eventos son best-effort)
                    _logger.LogError(ex,
                        "Error publishing ContentAssignedEvent for assignment {AssignmentId}: {Error}",
                        assignmentId, ex.Message);
                }
            }

            _logger.LogInformation(
                "Successfully assigned content to {Count} patients by psychologist: {PsychologistId}",
                command.PatientIds.Count, command.PsychologistId);

            return assignmentIds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating assignments for psychologist {PsychologistId}. Transaction will be rolled back.",
                command.PsychologistId);
            throw; // El UnitOfWork debería hacer rollback automáticamente
        }
    }
}