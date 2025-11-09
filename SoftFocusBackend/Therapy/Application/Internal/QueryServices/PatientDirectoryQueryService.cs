using SoftFocusBackend.Therapy.Domain.Model.Aggregates;
using SoftFocusBackend.Therapy.Domain.Model.Queries;
using SoftFocusBackend.Therapy.Domain.Repositories;
using SoftFocusBackend.Therapy.Domain.Model.ValueObjects;
using SoftFocusBackend.Therapy.Application.Internal.OutboundServices;
using SoftFocusBackend.Users.Domain.Model.Aggregates;

namespace SoftFocusBackend.Therapy.Application.Internal.QueryServices
{
    public class PatientDirectoryQueryService
    {
        private readonly ITherapeuticRelationshipRepository _relationshipRepository;
        private readonly IPatientFacade _patientFacade; // <-- NUEVA INYECCIÓN

        public PatientDirectoryQueryService(
            ITherapeuticRelationshipRepository relationshipRepository, 
            IPatientFacade patientFacade // <-- NUEVA INYECCIÓN
            )
        {
            _relationshipRepository = relationshipRepository;
            _patientFacade = patientFacade; // <-- NUEVA INYECCIÓN
        }

        public async Task<IEnumerable<PatientDirectory>> Handle(GetPatientDirectoryQuery query)
        {
            // 1. Obtener todas las relaciones del psicólogo (como antes)
            var relationships = await _relationshipRepository.GetByPsychologistIdAsync(query.PsychologistId);

            var directories = new List<PatientDirectory>();
            foreach (var rel in relationships)
            {
                // 2. Aplicar filtros (como antes)
                if (query.StatusFilter.HasValue && rel.Status != query.StatusFilter.Value) continue;

                // 3. (NUEVO) Obtener los datos del paciente (User) usando el Facade
                var patientUser = await _patientFacade.FetchPatientById(rel.PatientId);

                // Si el usuario no se encuentra (por alguna razón), saltamos este registro
                if (patientUser == null) continue; 

                // 4. (MODIFICADO) Crear el PatientDirectory usando el nuevo constructor
                // que combina los datos de la relación y del usuario.
                directories.Add(new PatientDirectory(rel, patientUser));
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