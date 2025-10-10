using SoftFocusBackend.Therapy.Domain.Model.Aggregates;
using SoftFocusBackend.Therapy.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Therapy.Domain.Repositories
{
    public interface ITherapeuticRelationshipRepository
    {
        Task<TherapeuticRelationship?> GetByIdAsync(string id);
        Task<TherapeuticRelationship?> GetByConnectionCodeAsync(ConnectionCode code);
        Task<IEnumerable<TherapeuticRelationship>> GetByPsychologistIdAsync(string psychologistId);
        Task<IEnumerable<TherapeuticRelationship>> GetByPatientIdAsync(string patientId);
        Task AddAsync(TherapeuticRelationship relationship);
        Task UpdateAsync(TherapeuticRelationship relationship);
    }
}