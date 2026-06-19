using SoftFocusBackend.Users.Domain.Model.Aggregates;

namespace SoftFocusBackend.Therapy.Application.Internal.OutboundServices
{
    // Define el contrato que Therapy necesita del módulo Users
    public interface IPatientFacade
    {
        // Usamos el agregado 'User' del Bounded Context de Users
        Task<User?> FetchPatientById(string patientId);

        // Obtiene cualquier usuario (paciente o psicólogo) por id. Usado por el feature de llamadas
        // para resolver el rol y el nombre de quien inicia la llamada.
        Task<User?> FetchUserById(string userId);
    }
}