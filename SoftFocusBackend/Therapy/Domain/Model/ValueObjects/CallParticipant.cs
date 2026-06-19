using MongoDB.Bson.Serialization.Attributes;

namespace SoftFocusBackend.Therapy.Domain.Model.ValueObjects
{
    /// <summary>
    /// A user invited to a call and their per-user state. Embedded inside the CallSession aggregate.
    /// </summary>
    public class CallParticipant
    {
        [BsonElement("user_id")]
        public string UserId { get; private set; }

        [BsonElement("is_caller")]
        public bool IsCaller { get; private set; }

        [BsonElement("has_accepted")]
        public bool HasAccepted { get; private set; }

        [BsonElement("has_rejected")]
        public bool HasRejected { get; private set; }

        [BsonElement("joined_at")]
        public DateTime? JoinedAt { get; private set; }

        [BsonElement("left_at")]
        public DateTime? LeftAt { get; private set; }

        // Required by the MongoDB serializer.
        public CallParticipant()
        {
            UserId = string.Empty;
        }

        public CallParticipant(string userId, bool isCaller)
        {
            UserId = userId ?? throw new ArgumentNullException(nameof(userId));
            IsCaller = isCaller;
            HasAccepted = isCaller; // the caller is implicitly in the call
            HasRejected = false;
            JoinedAt = isCaller ? DateTime.UtcNow : null;
        }

        public void Accept()
        {
            HasAccepted = true;
            HasRejected = false;
            JoinedAt ??= DateTime.UtcNow;
        }

        public void Reject()
        {
            HasRejected = true;
            HasAccepted = false;
        }

        public void Leave()
        {
            LeftAt = DateTime.UtcNow;
        }
    }
}
