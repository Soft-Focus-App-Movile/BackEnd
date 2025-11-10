using SoftFocusBackend.Shared.Domain.Events;

namespace SoftFocusBackend.Therapy.Domain.Model.Events;

/// <summary>
/// Evento: Se envió un mensaje en el chat terapéutico
/// </summary>
public class MessageSentEvent : DomainEvent
{
    public string MessageId { get; }
    public string RelationshipId { get; }
    public string SenderId { get; }
    public string ReceiverId { get; }
    public string Content { get; }
    public string MessageType { get; }
    public bool SenderIsPsychologist { get; }

    public MessageSentEvent(
        string messageId,
        string relationshipId,
        string senderId,
        string receiverId,
        string content,
        string messageType,
        bool senderIsPsychologist)
    {
        MessageId = messageId;
        RelationshipId = relationshipId;
        SenderId = senderId;
        ReceiverId = receiverId;
        Content = content;
        MessageType = messageType;
        SenderIsPsychologist = senderIsPsychologist;
    }
}