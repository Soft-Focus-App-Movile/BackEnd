using SoftFocusBackend.Therapy.Domain.Model.Aggregates;
using SoftFocusBackend.Therapy.Domain.Model.Commands;
using SoftFocusBackend.Therapy.Domain.Model.ValueObjects;
using SoftFocusBackend.Therapy.Domain.Repositories;

namespace SoftFocusBackend.Therapy.Application.Internal.CommandServices
{
    public class PatientTaskCommandService
    {
        private readonly IPatientTaskRepository _taskRepository;
        private readonly ITherapeuticRelationshipRepository _relationshipRepository;

        public PatientTaskCommandService(
            IPatientTaskRepository taskRepository,
            ITherapeuticRelationshipRepository relationshipRepository)
        {
            _taskRepository = taskRepository;
            _relationshipRepository = relationshipRepository;
        }

        /// <summary>
        /// Crea una tarea personalizada. Valida que exista una relación terapéutica
        /// activa entre el psicólogo y el paciente antes de permitir la asignación.
        /// </summary>
        public async Task<PatientTask> Handle(CreatePatientTaskCommand command)
        {
            var relationships = await _relationshipRepository.GetByPsychologistIdAsync(command.PsychologistId);
            var hasActiveRelationship = relationships.Any(r =>
                r.PatientId == command.PatientId && r.Status == TherapyStatus.Active && r.IsActive);

            if (!hasActiveRelationship)
                throw new UnauthorizedAccessException("No existe una relación activa con este paciente.");

            var task = PatientTask.Create(
                command.PsychologistId,
                command.PatientId,
                command.Title,
                command.Description);

            await _taskRepository.AddAsync(task);
            return task;
        }

        /// <summary>
        /// Marca una tarea como completada. Solo el paciente dueño de la tarea puede hacerlo.
        /// </summary>
        public async Task<PatientTask> HandleComplete(CompletePatientTaskCommand command)
        {
            var task = await _taskRepository.GetByIdAsync(command.TaskId);
            if (task == null)
                throw new InvalidOperationException("Tarea no encontrada.");

            if (!task.BelongsToPatient(command.PatientId))
                throw new UnauthorizedAccessException("Esta tarea no pertenece al usuario.");

            task.MarkAsCompleted();
            await _taskRepository.UpdateAsync(task);
            return task;
        }
    }
}
