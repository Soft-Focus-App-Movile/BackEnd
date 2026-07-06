namespace SoftFocusBackend.Therapy.Interfaces.REST.Resources
{
    public class CreatePatientTaskRequest
    {
        public string PatientId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
