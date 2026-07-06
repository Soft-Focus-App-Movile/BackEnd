namespace SoftFocusBackend.Therapy.Domain.Model.Commands
{
    public class CreatePatientTaskCommand
    {
        public string PsychologistId { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
