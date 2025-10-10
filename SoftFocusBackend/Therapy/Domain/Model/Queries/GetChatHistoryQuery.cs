namespace SoftFocusBackend.Therapy.Domain.Model.Queries
{
    public class GetChatHistoryQuery
    {
        public string RelationshipId { get; set; }
        public int Page { get; set; } = 1;
        public int Size { get; set; } = 20;
    }
}