namespace SoftFocusBackend.Therapy.Domain.Model.Commands
{
    public class SendChatMessageCommand
    {
        public string RelationshipId { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public string Content { get; set; }
        public string MessageType { get; set; }
    }
}