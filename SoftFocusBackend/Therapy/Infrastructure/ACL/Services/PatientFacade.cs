using SoftFocusBackend.Therapy.Application.Internal.OutboundServices; // Interfaz de Therapy
using SoftFocusBackend.Users.Domain.Model.Aggregates;
using UsersIUserFacade = SoftFocusBackend.Users.Application.Internal.OutboundServices.IUserFacade; 

namespace SoftFocusBackend.Therapy.Infrastructure.ACL.Services
{
    // Esta es la implementación (Capa Anti-Corrupción)
    public class PatientFacade : IPatientFacade // Implementa la interfaz de Therapy
    {
        private readonly UsersIUserFacade _usersFacade; // Inyecta la interfaz de Users

        public PatientFacade(UsersIUserFacade usersFacade)
        {
            _usersFacade = usersFacade;
        }

        // Este método cumple el contrato de IPatientFacade...
        public async Task<User?> FetchPatientById(string patientId)
        {
            // ...llamando al método real del Bounded Context de Users
            return await _usersFacade.GetUserByIdAsync(patientId);
        }
    }
}