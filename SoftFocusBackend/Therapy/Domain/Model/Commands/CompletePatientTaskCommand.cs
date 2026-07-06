namespace SoftFocusBackend.Therapy.Domain.Model.Commands
{
    public class CompletePatientTaskCommand
    {
        public string TaskId { get; set; } = string.Empty;
        public string PatientId { get; set; } = string.Empty;
    }
}
