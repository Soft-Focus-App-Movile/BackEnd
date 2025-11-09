using SoftFocusBackend.Therapy.Domain.Model.Aggregates;
using SoftFocusBackend.Therapy.Domain.Model.Queries;
using SoftFocusBackend.Therapy.Domain.Repositories;
using SoftFocusBackend.Therapy.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Therapy.Application.Internal.QueryServices
{
    public class PatientDirectoryQueryService
    {
        private readonly ITherapeuticRelationshipRepository _relationshipRepository;

        public PatientDirectoryQueryService(ITherapeuticRelationshipRepository relationshipRepository)
        {
            _relationshipRepository = relationshipRepository;
        }

        public async Task<IEnumerable<PatientDirectory>> Handle(GetPatientDirectoryQuery query)
        {
            var relationships = await _relationshipRepository.GetByPsychologistIdAsync(query.PsychologistId);

            var directories = new List<PatientDirectory>();
            foreach (var rel in relationships)
            {
                if (query.StatusFilter.HasValue && rel.Status != query.StatusFilter.Value) continue;

                directories.Add(new PatientDirectory
                {
                    Id = rel.Id,
                    PsychologistId = rel.PsychologistId,
                    PatientId = rel.PatientId,
                    Status = rel.Status,
                    StartDate = rel.StartDate,
                    SessionCount = rel.SessionCount
                });
            }

            return directories;
        }

        public async Task<object?> GetMyRelationship(GetMyRelationshipQuery query)
        {
            var relationships = await _relationshipRepository.GetByPatientIdAsync(query.PatientId);
            var activeRelationship = relationships.FirstOrDefault(r => r.Status == TherapyStatus.Active && r.IsActive);

            if (activeRelationship == null)
                return null;

            return new
            {
                id = activeRelationship.Id,
                psychologistId = activeRelationship.PsychologistId,
                patientId = activeRelationship.PatientId,
                startDate = activeRelationship.StartDate,
                status = activeRelationship.Status.ToString(),
                isActive = activeRelationship.IsActive,
                sessionCount = activeRelationship.SessionCount
            };
        }
    }
}