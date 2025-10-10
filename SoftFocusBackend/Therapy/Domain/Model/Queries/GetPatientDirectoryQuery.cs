using SoftFocusBackend.Therapy.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Therapy.Domain.Model.Queries
{
    public class GetPatientDirectoryQuery
    {
        public string PsychologistId { get; set; }
        public TherapyStatus? StatusFilter { get; set; }
    }
}