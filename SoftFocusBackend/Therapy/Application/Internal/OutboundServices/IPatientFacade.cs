using SoftFocusBackend.Users.Domain.Model.Aggregates;

namespace SoftFocusBackend.Therapy.Application.Internal.OutboundServices
{
    // Define el contrato que Therapy necesita del módulo Users
    public interface IPatientFacade
    {
        // Usamos el agregado 'User' del Bounded Context de Users
        Task<User?> FetchPatientById(string patientId);
    }
}