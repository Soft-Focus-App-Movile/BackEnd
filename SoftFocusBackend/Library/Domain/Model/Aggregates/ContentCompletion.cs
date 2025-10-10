using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SoftFocusBackend.Shared.Domain.Entities;

namespace SoftFocusBackend.Library.Domain.Model.Aggregates;

/// <summary>
/// Aggregate Root que registra la finalización de una asignación de contenido
/// Se crea cuando un paciente marca una asignación como completada
/// </summary>
public class ContentCompletion : BaseEntity
{
    /// <summary>
    /// ID de la asignación completada
    /// </summary>
    [BsonElement("assignmentId")]
    public string AssignmentId { get; set; } = string.Empty;

    /// <summary>
    /// ID del paciente que completó la asignación
    /// </summary>
    [BsonElement("patientId")]
    public string PatientId { get; set; } = string.Empty;

    /// <summary>
    /// Fecha en que se completó
    /// </summary>
    [BsonElement("completedAt")]
    public DateTime CompletedAt { get; set; }

    /// <summary>
    /// Constructor por defecto para MongoDB
    /// </summary>
    public ContentCompletion() { }

    /// <summary>
    /// Crea un nuevo registro de finalización
    /// </summary>
    public static ContentCompletion Create(
        string assignmentId,
        string patientId)
    {
        if (string.IsNullOrWhiteSpace(assignmentId))
            throw new ArgumentException("AssignmentId no puede estar vacío", nameof(assignmentId));

        if (string.IsNullOrWhiteSpace(patientId))
            throw new ArgumentException("PatientId no puede estar vacío", nameof(patientId));

        var now = DateTime.UtcNow;

        return new ContentCompletion
        {
            Id = ObjectId.GenerateNewId().ToString(),
            AssignmentId = assignmentId,
            PatientId = patientId,
            CompletedAt = now,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    /// <summary>
    /// Verifica si la finalización pertenece a un paciente específico
    /// </summary>
    public bool BelongsToPatient(string patientId)
    {
        return PatientId == patientId;
    }
}
