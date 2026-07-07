using SoftFocusBackend.Therapy.Domain.Model.Aggregates;

namespace SoftFocusBackend.Therapy.Domain.Repositories
{
    public interface IPatientTaskRepository
    {
        Task<PatientTask?> GetByIdAsync(string id);
        Task<IEnumerable<PatientTask>> GetByPatientIdAsync(string patientId);
        Task<IEnumerable<PatientTask>> GetByPsychologistAndPatientAsync(string psychologistId, string patientId);
        Task AddAsync(PatientTask task);
        Task UpdateAsync(PatientTask task);
    }
}
