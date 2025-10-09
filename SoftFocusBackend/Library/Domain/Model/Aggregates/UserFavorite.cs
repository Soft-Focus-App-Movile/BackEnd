using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SoftFocusBackend.Library.Domain.Model.ValueObjects;
using SoftFocusBackend.Shared.Domain.Entities;

namespace SoftFocusBackend.Library.Domain.Model.Aggregates;

/// <summary>
/// Aggregate Root que representa un contenido marcado como favorito por un usuario
/// Solo disponible para Usuario General y Paciente (no Psicólogo)
/// </summary>
public class UserFavorite : BaseEntity
{
    /// <summary>
    /// ID del usuario propietario del favorito
    /// </summary>
    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// ExternalId del contenido (ej: "tmdb-movie-27205")
    /// </summary>
    [BsonElement("contentId")]
    public string ContentId { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de contenido favorito
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
    /// Fecha en que se agregó a favoritos
    /// </summary>
    [BsonElement("addedAt")]
    public DateTime AddedAt { get; set; }

    /// <summary>
    /// Constructor por defecto para MongoDB
    /// </summary>
    public UserFavorite() { }

    /// <summary>
    /// Crea un nuevo favorito para un usuario
    /// </summary>
    public static UserFavorite Create(
        string userId,
        ContentItem content)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId no puede estar vacío", nameof(userId));

        if (content == null)
            throw new ArgumentNullException(nameof(content));

        var now = DateTime.UtcNow;

        return new UserFavorite
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = userId,
            ContentId = content.ExternalId,
            ContentType = content.ContentType,
            Content = content,
            AddedAt = now,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    /// <summary>
    /// Verifica si el favorito pertenece a un usuario específico
    /// </summary>
    public bool BelongsToUser(string userId)
    {
        return UserId == userId;
    }

    /// <summary>
    /// Actualiza el contenido embebido (útil si se actualiza el caché)
    /// </summary>
    public void UpdateContent(ContentItem updatedContent)
    {
        if (updatedContent == null)
            throw new ArgumentNullException(nameof(updatedContent));

        if (updatedContent.ExternalId != ContentId)
            throw new InvalidOperationException("El contenido no coincide con el favorito");

        Content = updatedContent;
        UpdatedAt = DateTime.UtcNow;
    }
}
