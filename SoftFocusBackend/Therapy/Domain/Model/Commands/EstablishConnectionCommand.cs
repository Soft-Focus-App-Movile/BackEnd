namespace SoftFocusBackend.Therapy.Domain.Model.Commands
{
    public class EstablishConnectionCommand
    {
        public string PatientId { get; set; }
        public string ConnectionCode { get; set; }
    }
}