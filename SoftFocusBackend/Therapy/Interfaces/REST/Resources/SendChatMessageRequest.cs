namespace SoftFocusBackend.Therapy.Interfaces.REST.Resources
{
    public class SendChatMessageRequest
    {
        public SendChatMessageRequest(string relationshipId, string receiverId, string content, string messageType)
        {
            RelationshipId = relationshipId;
            ReceiverId = receiverId;
            Content = content;
            MessageType = messageType;
        }

        public string RelationshipId { get; set; }
        public string ReceiverId { get; set; }
        public string Content { get; set; }
        public string MessageType { get; set; } = "text";
    }
}