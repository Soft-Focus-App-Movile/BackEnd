using SoftFocusBackend.Therapy.Domain.Model.ValueObjects;
using SoftFocusBackend.Users.Domain.Model.Aggregates;

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

        // Constructor privado para el mapeador de MongoDB
        private PatientDirectory() 
        {
            // Inicializar propiedades de solo lectura para evitar warnings
            PatientName = string.Empty;
            ProfilePhotoUrl = string.Empty;
        } 

        // CONSTRUCTOR PÚBLICO
        public PatientDirectory(TherapeuticRelationship relationship, User patient)
        {
            // Datos de la Relación (Therapy)
            Id = relationship.Id;
            PsychologistId = relationship.PsychologistId;
            PatientId = relationship.PatientId;
            Status = relationship.Status;
            StartDate = relationship.StartDate;
            SessionCount = relationship.SessionCount;

            // Datos del Paciente (Users)
            PatientName = patient.FullName;
            ProfilePhotoUrl = patient.ProfileImageUrl ?? string.Empty; // Asignar URL de foto
            Age = patient.DateOfBirth.HasValue ? CalculateAge(patient.DateOfBirth.Value) : 0; // Calcular edad
            
            // Este dato aún no existe en los modelos, se deja como null
            LastSessionDate = null; 
        }

        // Función helper privada para calcular la edad
        private static int CalculateAge(DateTime dateOfBirth)
        {
            var today = DateTime.UtcNow;
            var age = today.Year - dateOfBirth.Year;
            if (dateOfBirth.Date > today.AddYears(-age)) age--;
            return age;
        }
    }
}