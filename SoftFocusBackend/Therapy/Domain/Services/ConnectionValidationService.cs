using SoftFocusBackend.Therapy.Domain.Model.Aggregates;
using SoftFocusBackend.Therapy.Domain.Model.ValueObjects;
using SoftFocusBackend.Therapy.Domain.Repositories;
using SoftFocusBackend.Users.Infrastructure.Persistence.MongoDB.Repositories;

namespace SoftFocusBackend.Therapy.Domain.Services
{
    public class ConnectionValidationService : IConnectionValidationService
    {
        private readonly ITherapeuticRelationshipRepository _relationshipRepository;
        private readonly IPsychologistRepository _psychologistRepository;

        public ConnectionValidationService(
            
            ITherapeuticRelationshipRepository relationshipRepository,
            IPsychologistRepository psychologistRepository)
        {
            _relationshipRepository = relationshipRepository;
            _psychologistRepository = psychologistRepository;
        }

        public async Task<bool> ValidateCodeAsync(ConnectionCode code, string psychologistId)
        {
            var psychologist = await _psychologistRepository.FindByIdAsync(psychologistId);
            if (psychologist == null)
                return false;

            return psychologist.InvitationCode == code.Value &&
                   !psychologist.IsInvitationCodeExpired();
        }

        public async Task<TherapeuticRelationship> EstablishConnectionAsync(string patientId, ConnectionCode code)
        {
            // Find psychologist by invitation code
            var psychologist = await _psychologistRepository.FindByInvitationCodeAsync(code.Value);
            if (psychologist == null)
                throw new InvalidOperationException("Invalid invitation code");

            if (psychologist.IsInvitationCodeExpired())
                throw new InvalidOperationException("Invitation code has expired");

            // Check if relationship already exists
            var existingRelationships = await _relationshipRepository.GetByPatientIdAsync(patientId);
            var activeRelationship = existingRelationships.FirstOrDefault(r => r.IsActive);
            if (activeRelationship != null)
                throw new InvalidOperationException("Patient already has an active therapeutic relationship");

            // Create new relationship - constructor expects (psychologistId, connectionCode, patientId)
            var relationship = new TherapeuticRelationship(psychologist.Id, code, patientId);
            return relationship;
        }
    }
}
