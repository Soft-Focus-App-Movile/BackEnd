using MongoDB.Bson.Serialization.Attributes;
using SoftFocusBackend.Shared.Domain.Entities;
using SoftFocusBackend.Therapy.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Therapy.Domain.Model.Aggregates
{
    public class ChatMessage : BaseEntity
    {
        [BsonElement("chat_message_id")]
        public string Id { get; private set; }
        
        [BsonElement("relationship_id")]
        public string RelationshipId { get; private set; } // Link to TherapeuticRelationship
        
        [BsonElement("sender_id")]
        public string SenderId { get; private set; }
        
        [BsonElement("receiver_id")]
        public string ReceiverId { get; private set; }
        
        [BsonElement("content")]
        public MessageContent Content { get; private set; }
        
        [BsonElement("timestamp")]
        public DateTime Timestamp { get; private set; }
        
        [BsonElement("is_read")]
        public bool IsRead { get; private set; }
        
        [BsonElement("message_type")]
        public string MessageType { get; private set; } // e.g., "text", "image", "document"

        public ChatMessage(string relationshipId, string senderId, string receiverId, MessageContent content, string messageType)
        {
            Id = Guid.NewGuid().ToString();
            RelationshipId = relationshipId ?? throw new ArgumentNullException(nameof(relationshipId));
            SenderId = senderId ?? throw new ArgumentNullException(nameof(senderId));
            ReceiverId = receiverId ?? throw new ArgumentNullException(nameof(receiverId));
            Content = content ?? throw new ArgumentNullException(nameof(content));
            MessageType = messageType ?? "text";
            Timestamp = DateTime.UtcNow;
            IsRead = false;
        }

        public void MarkAsRead()
        {
            IsRead = true;
        }
    }
}