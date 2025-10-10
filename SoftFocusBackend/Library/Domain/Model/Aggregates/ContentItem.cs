using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SoftFocusBackend.Library.Domain.Model.ValueObjects;
using SoftFocusBackend.Shared.Domain.Entities;

namespace SoftFocusBackend.Library.Domain.Model.Aggregates;

/// <summary>
/// Aggregate Root que representa un contenido multimedia cacheado de APIs externas
/// Incluye TTL automático mediante índice de MongoDB
/// </summary>
public class ContentItem : BaseEntity
{
    /// <summary>
    /// Identificador externo único (ej: "tmdb-movie-27205", "spotify-track-xxx")
    /// </summary>
    [BsonElement("externalId")]
    public string ExternalId { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de contenido (Movie, Series, Music, Video, Place)
    /// </summary>
    [BsonElement("contentType")]
    [BsonRepresentation(BsonType.String)]
    public ContentType ContentType { get; set; }

    /// <summary>
    /// Metadata completa del contenido
    /// </summary>
    [BsonElement("metadata")]
    public ContentMetadata Metadata { get; set; } = new();

    /// <summary>
    /// Tags emocionales asociados al contenido
    /// </summary>
    [BsonElement("emotionalTags")]
    [BsonRepresentation(BsonType.String)]
    public List<EmotionalTag> EmotionalTags { get; set; } = new();

    /// <summary>
    /// URL externa al contenido original
    /// </summary>
    [BsonElement("externalUrl")]
    public string ExternalUrl { get; set; } = string.Empty;

    /// <summary>
    /// Fecha en que se cacheó el contenido
    /// </summary>
    [BsonElement("cachedAt")]
    public DateTime CachedAt { get; set; }

    /// <summary>
    /// Fecha de expiración del caché (usada por MongoDB TTL index)
    /// </summary>
    [BsonElement("expiresAt")]
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Constructor por defecto para MongoDB
    /// </summary>
    public ContentItem() { }

    /// <summary>
    /// Crea un nuevo ContentItem con TTL basado en el tipo de contenido
    /// </summary>
    public static ContentItem Create(
        string externalId,
        ContentType contentType,
        ContentMetadata metadata,
        List<EmotionalTag> emotionalTags,
        string externalUrl,
        int cacheDurationHours)
    {
        if (string.IsNullOrWhiteSpace(externalId))
            throw new ArgumentException("ExternalId no puede estar vacío", nameof(externalId));

        if (metadata == null)
            throw new ArgumentNullException(nameof(metadata));

        var now = DateTime.UtcNow;

        return new ContentItem
        {
            Id = ObjectId.GenerateNewId().ToString(),
            ExternalId = externalId,
            ContentType = contentType,
            Metadata = metadata,
            EmotionalTags = emotionalTags ?? new List<EmotionalTag>(),
            ExternalUrl = externalUrl,
            CachedAt = now,
            ExpiresAt = now.AddHours(cacheDurationHours),
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    /// <summary>
    /// Actualiza la metadata del contenido y extiende el TTL
    /// </summary>
    public void UpdateMetadata(ContentMetadata newMetadata, int cacheDurationHours)
    {
        if (newMetadata == null)
            throw new ArgumentNullException(nameof(newMetadata));

        Metadata = newMetadata;
        UpdatedAt = DateTime.UtcNow;
        ExpiresAt = DateTime.UtcNow.AddHours(cacheDurationHours);
    }

    /// <summary>
    /// Agrega un tag emocional si no existe
    /// </summary>
    public void AddEmotionalTag(EmotionalTag tag)
    {
        if (!EmotionalTags.Contains(tag))
        {
            EmotionalTags.Add(tag);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Verifica si el contenido ha expirado
    /// </summary>
    public bool IsExpired()
    {
        return DateTime.UtcNow >= ExpiresAt;
    }

    /// <summary>
    /// Verifica si el contenido tiene un tag emocional específico
    /// </summary>
    public bool HasEmotionalTag(EmotionalTag tag)
    {
        return EmotionalTags.Contains(tag);
    }

    /// <summary>
    /// Refresca el caché extendiendo el tiempo de expiración
    /// Útil cuando se accede al contenido para mantenerlo en caché
    /// </summary>
    public void RefreshCache(int? cacheDurationHours = null)
    {
        var duration = cacheDurationHours ?? GetDefaultCacheDuration();
        ExpiresAt = DateTime.UtcNow.AddHours(duration);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Obtiene la duración de caché por defecto según el tipo de contenido
    /// </summary>
    private int GetDefaultCacheDuration()
    {
        return ContentType switch
        {
            ContentType.Movie => 24,
            ContentType.Series => 24,
            ContentType.Music => 24,
            ContentType.Video => 24,
            ContentType.Place => 6,
            _ => 12
        };
    }
}
