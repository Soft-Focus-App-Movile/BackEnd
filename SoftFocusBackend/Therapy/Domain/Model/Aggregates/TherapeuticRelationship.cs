using MongoDB.Bson.Serialization.Attributes;
using SoftFocusBackend.Shared.Domain.Entities;
using SoftFocusBackend.Therapy.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Therapy.Domain.Model.Aggregates
{
    public class TherapeuticRelationship : BaseEntity
    {
        [BsonElement("therapeutic_relationship_id")]
        public string Id { get; private set; }
        
        [BsonElement("psychologist_id")]
        public string PsychologistId { get; private set; }
        
        [BsonElement("patient_id")]
        public string PatientId { get; private set; }
        
        [BsonElement("connection_code")]
        public ConnectionCode ConnectionCode { get; private set; }
        
        [BsonElement("start_date")]
        public DateTime StartDate { get; private set; }
        
        [BsonElement("end_date")]
        public DateTime? EndDate { get; private set; }
        
        [BsonElement("status")]
        public TherapyStatus Status { get; private set; }
        
        [BsonElement("is_active")]
        public bool IsActive { get; private set; }
        
        [BsonElement("session_count")]
        public int SessionCount { get; private set; }

        // Constructor for new relationships
        public TherapeuticRelationship(string psychologistId, ConnectionCode connectionCode, string patientId)
        {
            Id = Guid.NewGuid().ToString();
            PsychologistId = psychologistId ?? throw new ArgumentNullException(nameof(psychologistId));
            ConnectionCode = connectionCode ?? throw new ArgumentNullException(nameof(connectionCode));
            PatientId = patientId;
            StartDate = DateTime.UtcNow;
            Status = TherapyStatus.Pending;
            IsActive = false;
            SessionCount = 0;
        }

        // Method to establish connection
        public void Establish(string patientId)
        {
            if (Status != TherapyStatus.Pending)
                throw new InvalidOperationException("Relationship is not pending.");

            PatientId = patientId ?? throw new ArgumentNullException(nameof(patientId));
            Status = TherapyStatus.Active;
            IsActive = true;
        }

        public void IncrementSession()
        {
            if (!IsActive)
                throw new InvalidOperationException("Cannot increment session on inactive relationship.");
            SessionCount++;
        }

        public void Pause()
        {
            if (Status != TherapyStatus.Active)
                throw new InvalidOperationException("Can only pause active relationships.");
            Status = TherapyStatus.Paused;
            IsActive = false;
        }

        public void Terminate()
        {
            if (Status == TherapyStatus.Terminated)
                throw new InvalidOperationException("Relationship already terminated.");
            Status = TherapyStatus.Terminated;
            IsActive = false;
            EndDate = DateTime.UtcNow;
        }
    }
}