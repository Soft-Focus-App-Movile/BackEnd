using SoftFocusBackend.Therapy.Domain.Model.Aggregates;
using SoftFocusBackend.Therapy.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Therapy.Domain.Services
{
    public interface IConnectionValidationService
    {
        Task<bool> ValidateCodeAsync(ConnectionCode code, string psychologistId);
        Task<TherapeuticRelationship> EstablishConnectionAsync(string patientId, ConnectionCode code);
    }
}