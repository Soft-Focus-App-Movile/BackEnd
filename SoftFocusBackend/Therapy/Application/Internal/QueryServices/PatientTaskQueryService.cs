using SoftFocusBackend.Therapy.Domain.Model.Aggregates;
using SoftFocusBackend.Therapy.Domain.Repositories;

namespace SoftFocusBackend.Therapy.Application.Internal.QueryServices
{
    public class PatientTaskQueryService
    {
        private readonly IPatientTaskRepository _taskRepository;

        public PatientTaskQueryService(IPatientTaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }

        /// <summary>Tareas que un psicólogo asignó a un paciente concreto (vista del psicólogo).</summary>
        public async Task<IEnumerable<PatientTask>> GetByPsychologistAndPatient(string psychologistId, string patientId)
        {
            return await _taskRepository.GetByPsychologistAndPatientAsync(psychologistId, patientId);
        }

        /// <summary>Tareas asignadas al paciente autenticado (vista del paciente).</summary>
        public async Task<IEnumerable<PatientTask>> GetForPatient(string patientId)
        {
            return await _taskRepository.GetByPatientIdAsync(patientId);
        }
    }
}
