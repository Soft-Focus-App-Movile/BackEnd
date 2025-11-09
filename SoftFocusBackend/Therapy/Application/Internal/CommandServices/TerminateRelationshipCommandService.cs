using SoftFocusBackend.Therapy.Domain.Model.Commands;
using SoftFocusBackend.Therapy.Domain.Repositories;

namespace SoftFocusBackend.Therapy.Application.Internal.CommandServices
{
    public class TerminateRelationshipCommandService
    {
        private readonly ITherapeuticRelationshipRepository _relationshipRepository;

        public TerminateRelationshipCommandService(ITherapeuticRelationshipRepository relationshipRepository)
        {
            _relationshipRepository = relationshipRepository;
        }

        public async Task Handle(TerminateRelationshipCommand command)
        {
            // Get all relationships for the user (could be patient or psychologist)
            var patientRelationships = await _relationshipRepository.GetByPatientIdAsync(command.UserId);
            var psychologistRelationships = await _relationshipRepository.GetByPsychologistIdAsync(command.UserId);

            var allRelationships = patientRelationships.Concat(psychologistRelationships);

            var relationship = allRelationships.FirstOrDefault(r => r.Id == command.RelationshipId);

            if (relationship == null)
                throw new InvalidOperationException("Relationship not found or you don't have access to it");

            // Verify the user is part of this relationship
            if (relationship.PatientId != command.UserId && relationship.PsychologistId != command.UserId)
                throw new UnauthorizedAccessException("You are not authorized to terminate this relationship");

            relationship.Terminate();

            await _relationshipRepository.UpdateAsync(relationship);
        }
    }
}
