using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SoftFocusBackend.Shared.Domain.Entities;

namespace SoftFocusBackend.Therapy.Domain.Model.Aggregates
{
    /// <summary>
    /// Aggregate Root que representa una tarea/propósito de texto libre que un
    /// psicólogo escribe y asigna a un paciente.
    ///
    /// A diferencia de <c>ContentAssignment</c> (Library), esta tarea NO está atada
    /// a contenido de la biblioteca: es simplemente un título y una descripción que
    /// el psicólogo redacta. Se guarda en su propia colección (<c>patient_tasks</c>),
    /// por lo que no impacta ninguna otra entidad del sistema.
    /// </summary>
    public class PatientTask : BaseEntity
    {
        /// <summary>ID del psicólogo que crea la tarea.</summary>
        [BsonElement("psychologistId")]
        public string PsychologistId { get; set; } = string.Empty;

        /// <summary>ID del paciente al que se le asigna la tarea.</summary>
        [BsonElement("patientId")]
        public string PatientId { get; set; } = string.Empty;

        /// <summary>Título / propósito de la tarea (obligatorio).</summary>
        [BsonElement("title")]
        public string Title { get; set; } = string.Empty;

        /// <summary>Descripción o detalle de la tarea (opcional).</summary>
        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>Indica si el paciente ya completó la tarea.</summary>
        [BsonElement("isCompleted")]
        public bool IsCompleted { get; set; }

        /// <summary>Fecha en que se completó (null si sigue pendiente).</summary>
        [BsonElement("completedAt")]
        [BsonIgnoreIfNull]
        public DateTime? CompletedAt { get; set; }

        /// <summary>Fecha en que el psicólogo asignó la tarea.</summary>
        [BsonElement("assignedAt")]
        public DateTime AssignedAt { get; set; }

        public PatientTask() { }

        /// <summary>
        /// Crea una nueva tarea personalizada validando los campos obligatorios.
        /// </summary>
        public static PatientTask Create(string psychologistId, string patientId, string title, string description)
        {
            if (string.IsNullOrWhiteSpace(psychologistId))
                throw new ArgumentException("PsychologistId no puede estar vacío", nameof(psychologistId));

            if (string.IsNullOrWhiteSpace(patientId))
                throw new ArgumentException("PatientId no puede estar vacío", nameof(patientId));

            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("El título de la tarea no puede estar vacío", nameof(title));

            var now = DateTime.UtcNow;

            return new PatientTask
            {
                Id = ObjectId.GenerateNewId().ToString(),
                PsychologistId = psychologistId,
                PatientId = patientId,
                Title = title.Trim(),
                Description = description?.Trim() ?? string.Empty,
                IsCompleted = false,
                CompletedAt = null,
                AssignedAt = now,
                CreatedAt = now,
                UpdatedAt = now
            };
        }

        /// <summary>Marca la tarea como completada.</summary>
        public void MarkAsCompleted()
        {
            if (IsCompleted)
                throw new InvalidOperationException("La tarea ya está completada");

            IsCompleted = true;
            CompletedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool BelongsToPatient(string patientId) => PatientId == patientId;

        public bool CreatedByPsychologist(string psychologistId) => PsychologistId == psychologistId;
    }
}
