namespace SoftFocusBackend.Therapy.Domain.Model.Commands
{
    public class TerminateRelationshipCommand
    {
        public string UserId { get; set; }
        public string RelationshipId { get; set; }
    }
}
