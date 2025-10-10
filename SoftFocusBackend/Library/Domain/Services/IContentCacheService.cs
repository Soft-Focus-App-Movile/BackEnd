using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Library.Domain.Services;

/// <summary>
/// Servicio de dominio para gestionar el caché de contenidos
/// </summary>
public interface IContentCacheService
{
    /// <summary>
    /// Obtiene contenido del caché o de la API externa si no existe/expiró
    /// </summary>
    Task<ContentItem?> GetOrFetchContentAsync(
        string externalId,
        ContentType contentType);

    /// <summary>
    /// Guarda o actualiza contenido en el caché
    /// </summary>
    Task<ContentItem> CacheContentAsync(
        ContentItem content,
        int cacheDurationHours);

    /// <summary>
    /// Verifica si un contenido está en caché y no ha expirado
    /// </summary>
    Task<bool> IsCachedAndValidAsync(string externalId);

    /// <summary>
    /// Invalida (elimina) un contenido del caché
    /// </summary>
    Task InvalidateCacheAsync(string externalId);

    /// <summary>
    /// Obtiene la duración de caché apropiada según el tipo de contenido
    /// </summary>
    int GetCacheDurationForType(ContentType contentType);

    /// <summary>
    /// Limpia contenidos expirados del caché
    /// </summary>
    Task<int> CleanExpiredCacheAsync();
}
