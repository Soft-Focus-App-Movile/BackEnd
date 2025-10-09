using SoftFocusBackend.Therapy.Domain.Model.Aggregates;
using SoftFocusBackend.Therapy.Domain.Model.Commands;
using SoftFocusBackend.Therapy.Domain.Model.ValueObjects;
using SoftFocusBackend.Therapy.Domain.Repositories;
using SoftFocusBackend.Therapy.Domain.Services;

namespace SoftFocusBackend.Therapy.Application.Internal.CommandServices
{
    public class EstablishConnectionCommandService
    {
        private readonly ITherapeuticRelationshipRepository _relationshipRepository;
        private readonly IConnectionValidationService _validationService;

        public EstablishConnectionCommandService(
            ITherapeuticRelationshipRepository relationshipRepository,
            IConnectionValidationService validationService)
        {
            _relationshipRepository = relationshipRepository;
            _validationService = validationService;
        }

        public async Task<TherapeuticRelationship> Handle(EstablishConnectionCommand command)
        {
            var code = ConnectionCode.Create(command.ConnectionCode);

            var relationship = await _validationService.EstablishConnectionAsync(command.PatientId, code);

            await _relationshipRepository.AddAsync(relationship);

            return relationship;
        }
    }
}