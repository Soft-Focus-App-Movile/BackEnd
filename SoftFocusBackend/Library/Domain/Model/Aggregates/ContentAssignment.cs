using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SoftFocusBackend.Library.Domain.Model.ValueObjects;
using SoftFocusBackend.Shared.Domain.Entities;

namespace SoftFocusBackend.Library.Domain.Model.Aggregates;

/// <summary>
/// Aggregate Root que representa contenido asignado por un psicólogo a un paciente
/// Un psicólogo puede asignar el mismo contenido a múltiples pacientes
/// </summary>
public class ContentAssignment : BaseEntity
{
    /// <summary>
    /// ID del psicólogo que asigna el contenido
    /// </summary>
    [BsonElement("psychologistId")]
    public string PsychologistId { get; set; } = string.Empty;

    /// <summary>
    /// ID del paciente que recibe la asignación
    /// </summary>
    [BsonElement("patientId")]
    public string PatientId { get; set; } = string.Empty;

    /// <summary>
    /// ExternalId del contenido asignado
    /// </summary>
    [BsonElement("contentId")]
    public string ContentId { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de contenido asignado
    /// </summary>
    [BsonElement("contentType")]
    [BsonRepresentation(BsonType.String)]
    public ContentType ContentType { get; set; }

    /// <summary>
    /// Contenido embebido con metadata completa
    /// </summary>
    [BsonElement("content")]
    public ContentItem Content { get; set; } = new();

    /// <summary>
    /// Notas o instrucciones del psicólogo para el paciente
    /// </summary>
    [BsonElement("notes")]
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Fecha en que se realizó la asignación
    /// </summary>
    [BsonElement("assignedAt")]
    public DateTime AssignedAt { get; set; }

    /// <summary>
    /// Indica si el paciente completó la actividad
    /// </summary>
    [BsonElement("isCompleted")]
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Fecha en que se completó (null si no está completado)
    /// </summary>
    [BsonElement("completedAt")]
    [BsonIgnoreIfNull]
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Constructor por defecto para MongoDB
    /// </summary>
    public ContentAssignment() { }

    /// <summary>
    /// Crea una nueva asignación de contenido
    /// </summary>
    public static ContentAssignment Create(
        string psychologistId,
        string patientId,
        ContentItem content,
        string notes)
    {
        if (string.IsNullOrWhiteSpace(psychologistId))
            throw new ArgumentException("PsychologistId no puede estar vacío", nameof(psychologistId));

        if (string.IsNullOrWhiteSpace(patientId))
            throw new ArgumentException("PatientId no puede estar vacío", nameof(patientId));

        if (content == null)
            throw new ArgumentNullException(nameof(content));

        var now = DateTime.UtcNow;

        return new ContentAssignment
        {
            Id = ObjectId.GenerateNewId().ToString(),
            PsychologistId = psychologistId,
            PatientId = patientId,
            ContentId = content.ExternalId,
            ContentType = content.ContentType,
            Content = content,
            Notes = notes ?? string.Empty,
            AssignedAt = now,
            IsCompleted = false,
            CompletedAt = null,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    /// <summary>
    /// Marca la asignación como completada
    /// </summary>
    public void MarkAsCompleted()
    {
        if (IsCompleted)
            throw new InvalidOperationException("La asignación ya está completada");

        IsCompleted = true;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Verifica si la asignación pertenece a un paciente específico
    /// </summary>
    public bool BelongsToPatient(string patientId)
    {
        return PatientId == patientId;
    }

    /// <summary>
    /// Verifica si la asignación fue creada por un psicólogo específico
    /// </summary>
    public bool CreatedByPsychologist(string psychologistId)
    {
        return PsychologistId == psychologistId;
    }

    /// <summary>
    /// Actualiza las notas de la asignación
    /// </summary>
    public void UpdateNotes(string newNotes)
    {
        Notes = newNotes ?? string.Empty;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Actualiza el contenido embebido (útil si se actualiza el caché)
    /// </summary>
    public void UpdateContent(ContentItem updatedContent)
    {
        if (updatedContent == null)
            throw new ArgumentNullException(nameof(updatedContent));

        if (updatedContent.ExternalId != ContentId)
            throw new InvalidOperationException("El contenido no coincide con la asignación");

        Content = updatedContent;
        UpdatedAt = DateTime.UtcNow;
    }
}
