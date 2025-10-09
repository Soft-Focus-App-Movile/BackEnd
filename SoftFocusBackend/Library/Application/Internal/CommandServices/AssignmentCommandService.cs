using Microsoft.Extensions.Logging;
using SoftFocusBackend.Library.Application.ACL.Services;
using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Model.Commands;
using SoftFocusBackend.Library.Domain.Repositories;
using SoftFocusBackend.Shared.Domain.Repositories;
using SoftFocusBackend.Users.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Library.Application.Internal.CommandServices;

public class AssignmentCommandService : IAssignmentCommandService
{
    private readonly IContentAssignmentRepository _assignmentRepository;
    private readonly IContentItemRepository _contentRepository;
    private readonly IUserIntegrationService _userIntegration;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AssignmentCommandService> _logger;

    public AssignmentCommandService(
        IContentAssignmentRepository assignmentRepository,
        IContentItemRepository contentRepository,
        IUserIntegrationService userIntegration,
        IUnitOfWork unitOfWork,
        ILogger<AssignmentCommandService> logger)
    {
        _assignmentRepository = assignmentRepository;
        _contentRepository = contentRepository;
        _userIntegration = userIntegration;
        _unitOfWork = unitOfWork;
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

        // Validar que todos los pacientes existan y pertenezcan al psicólogo
        foreach (var patientId in command.PatientIds)
        {
            var patientExists = await _userIntegration.ValidateUserExistsAsync(patientId);
            if (!patientExists)
            {
                throw new InvalidOperationException($"Paciente no encontrado: {patientId}");
            }

            var belongsToPsychologist = await _userIntegration.ValidatePatientBelongsToPsychologistAsync(
                patientId, command.PsychologistId);

            if (!belongsToPsychologist)
            {
                throw new UnauthorizedAccessException(
                    $"El paciente {patientId} no pertenece a este psicólogo");
            }
        }

        // Obtener el contenido
        var content = await _contentRepository.FindByExternalIdAsync(command.ContentId);
        if (content == null)
        {
            throw new InvalidOperationException("Contenido no encontrado");
        }

        // Crear asignaciones para cada paciente
        var assignmentIds = new List<string>();

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
        }

        await _unitOfWork.CompleteAsync();

        _logger.LogInformation(
            "Content assigned to {Count} patients by psychologist: {PsychologistId}",
            command.PatientIds.Count, command.PsychologistId);

        return assignmentIds;
    }
}
