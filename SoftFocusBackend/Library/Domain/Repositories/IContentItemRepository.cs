using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Model.ValueObjects;
using SoftFocusBackend.Shared.Domain.Repositories;

namespace SoftFocusBackend.Library.Domain.Repositories;

/// <summary>
/// Repositorio para gestionar el caché de contenidos multimedia
/// </summary>
public interface IContentItemRepository : IBaseRepository<ContentItem>
{
    /// <summary>
    /// Busca un contenido por su ExternalId
    /// </summary>
    Task<ContentItem?> FindByExternalIdAsync(string externalId);

    /// <summary>
    /// Busca múltiples contenidos por tipo y tags emocionales
    /// </summary>
    Task<IEnumerable<ContentItem>> FindByTypeAndEmotionAsync(
        ContentType contentType,
        EmotionalTag emotion,
        int limit = 20);

    /// <summary>
    /// Busca contenidos por tipo (sin filtro de emoción)
    /// </summary>
    Task<IEnumerable<ContentItem>> FindByTypeAsync(
        ContentType contentType,
        int limit = 20);

    /// <summary>
    /// Busca contenidos que no hayan expirado
    /// </summary>
    Task<IEnumerable<ContentItem>> FindNonExpiredAsync(int limit = 100);

    /// <summary>
    /// Verifica si existe un contenido por ExternalId
    /// </summary>
    Task<bool> ExistsByExternalIdAsync(string externalId);

    /// <summary>
    /// Elimina contenidos expirados (aunque MongoDB TTL lo hace automáticamente)
    /// </summary>
    Task<int> RemoveExpiredAsync();
}
