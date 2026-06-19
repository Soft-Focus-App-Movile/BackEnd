using MongoDB.Bson.Serialization.Attributes;
using SoftFocusBackend.Shared.Domain.Entities;
using SoftFocusBackend.Therapy.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Therapy.Domain.Model.Aggregates
{
    /// <summary>
    /// A call between a patient and a psychologist (Direct) or a psychologist and all of their
    /// active patients (Group). Holds the Agora channel name and the per-participant call state.
    /// The actual audio/video flows peer-to-peer through Agora; this aggregate only tracks signaling
    /// and history.
    /// </summary>
    public class CallSession : BaseEntity
    {
        [BsonElement("channel_name")]
        public string ChannelName { get; private set; }

        [BsonElement("caller_id")]
        public string CallerId { get; private set; }

        [BsonElement("caller_role")]
        public string CallerRole { get; private set; } // "Patient" | "Psychologist"

        [BsonElement("call_type")]
        public CallType CallType { get; private set; }

        [BsonElement("mode")]
        public CallMode Mode { get; private set; }

        [BsonElement("relationship_id")]
        public string? RelationshipId { get; private set; } // set for Direct calls

        [BsonElement("status")]
        public CallStatus Status { get; private set; }

        [BsonElement("participants")]
        public List<CallParticipant> Participants { get; private set; }

        [BsonElement("started_at")]
        public DateTime StartedAt { get; private set; }

        [BsonElement("answered_at")]
        public DateTime? AnsweredAt { get; private set; }

        [BsonElement("ended_at")]
        public DateTime? EndedAt { get; private set; }

        // Required by the MongoDB serializer.
        public CallSession()
        {
            ChannelName = string.Empty;
            CallerId = string.Empty;
            CallerRole = string.Empty;
            Participants = new List<CallParticipant>();
        }

        public CallSession(
            string channelName,
            string callerId,
            string callerRole,
            CallType callType,
            CallMode mode,
            string? relationshipId,
            IEnumerable<string> inviteeIds)
        {
            Id = MongoDB.Bson.ObjectId.GenerateNewId().ToString();
            ChannelName = channelName ?? throw new ArgumentNullException(nameof(channelName));
            CallerId = callerId ?? throw new ArgumentNullException(nameof(callerId));
            CallerRole = callerRole ?? throw new ArgumentNullException(nameof(callerRole));
            CallType = callType;
            Mode = mode;
            RelationshipId = relationshipId;
            Status = CallStatus.Ringing;
            StartedAt = DateTime.UtcNow;

            Participants = new List<CallParticipant> { new CallParticipant(callerId, isCaller: true) };
            foreach (var inviteeId in inviteeIds.Where(id => id != callerId).Distinct())
            {
                Participants.Add(new CallParticipant(inviteeId, isCaller: false));
            }

            if (Participants.Count < 2)
                throw new InvalidOperationException("A call must have at least one invitee.");
        }

        public IEnumerable<string> InviteeIds =>
            Participants.Where(p => !p.IsCaller).Select(p => p.UserId);

        public bool IsParticipant(string userId) =>
            Participants.Any(p => p.UserId == userId);

        public void Accept(string userId)
        {
            var participant = GetParticipantOrThrow(userId);
            if (Status is CallStatus.Ended or CallStatus.Cancelled or CallStatus.Missed)
                throw new InvalidOperationException("Call is no longer active.");

            participant.Accept();
            if (Status == CallStatus.Ringing)
            {
                Status = CallStatus.Ongoing;
                AnsweredAt = DateTime.UtcNow;
            }
        }

        public void Reject(string userId)
        {
            var participant = GetParticipantOrThrow(userId);
            participant.Reject();

            // Direct call: a rejection by the single invitee ends the call as Rejected.
            if (Status == CallStatus.Ringing &&
                Participants.Where(p => !p.IsCaller).All(p => p.HasRejected))
            {
                Status = CallStatus.Rejected;
                EndedAt = DateTime.UtcNow;
            }
        }

        public void Leave(string userId)
        {
            var participant = GetParticipantOrThrow(userId);
            participant.Leave();

            // When everyone who accepted has left, the call is over.
            var activeRemain = Participants.Any(p =>
                (p.HasAccepted) && p.LeftAt == null);
            if (Status == CallStatus.Ongoing && !activeRemain)
            {
                Status = CallStatus.Ended;
                EndedAt = DateTime.UtcNow;
            }
        }

        /// <summary>Caller cancels before anyone answered, or any party explicitly ends the call.</summary>
        public void End(string userId)
        {
            GetParticipantOrThrow(userId);

            if (Status is CallStatus.Ended or CallStatus.Rejected or CallStatus.Cancelled or CallStatus.Missed)
                return;

            EndedAt = DateTime.UtcNow;
            Status = Status == CallStatus.Ringing ? CallStatus.Cancelled : CallStatus.Ended;
        }

        public void MarkMissed()
        {
            if (Status != CallStatus.Ringing) return;
            Status = CallStatus.Missed;
            EndedAt = DateTime.UtcNow;
        }

        private CallParticipant GetParticipantOrThrow(string userId)
        {
            var participant = Participants.FirstOrDefault(p => p.UserId == userId);
            if (participant == null)
                throw new UnauthorizedAccessException("User is not a participant of this call.");
            return participant;
        }
    }
}
