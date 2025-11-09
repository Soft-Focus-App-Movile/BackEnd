using SoftFocusBackend.Therapy.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Therapy.Domain.Model.Aggregates
{
    public class PatientDirectory
    {
        public string Id { get; internal set; }
        public string PsychologistId { get; internal set; }
        public string PatientId { get; internal set; }
        public string PatientName { get; private set; }
        public int Age { get; private set; }
        public string ProfilePhotoUrl { get; private set; }
        public TherapyStatus Status { get; set; }
        public DateTime StartDate { get; set; }
        public int SessionCount { get; set; }
        public DateTime? LastSessionDate { get; private set; }

        // Constructor would be populated from queries integrating with User bounded context
    }
}