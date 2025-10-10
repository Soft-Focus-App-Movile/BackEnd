using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Model.ValueObjects;
using SoftFocusBackend.Library.Domain.Repositories;
using SoftFocusBackend.Library.Domain.Services;
using SoftFocusBackend.Library.Infrastructure.Configuration;

namespace SoftFocusBackend.Library.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de caché de contenidos
/// Gestiona el almacenamiento y recuperación de contenido desde MongoDB
/// </summary>
public class ContentCacheService : IContentCacheService
{
    private readonly IContentItemRepository _contentRepository;
    private readonly LibraryCacheSettings _cacheSettings;
    private readonly ILogger<ContentCacheService> _logger;

    public ContentCacheService(
        IContentItemRepository contentRepository,
        IOptions<LibraryCacheSettings> cacheSettings,
        ILogger<ContentCacheService> logger)
    {
        _contentRepository = contentRepository;
        _cacheSettings = cacheSettings.Value;
        _logger = logger;
    }

    public async Task<ContentItem?> GetOrFetchContentAsync(
        string externalId,
        ContentType contentType)
    {
        try
        {
            // Intentar obtener del caché
            var cachedContent = await _contentRepository.FindByExternalIdAsync(externalId);

            // Si existe y no ha expirado, retornarlo
            if (cachedContent != null && !cachedContent.IsExpired())
            {
                _logger.LogDebug("Content found in cache: {ExternalId}", externalId);
                return cachedContent;
            }

            // Si existe pero expiró, eliminarlo
            if (cachedContent != null && cachedContent.IsExpired())
            {
                _logger.LogDebug("Content expired, removing from cache: {ExternalId}", externalId);
                _contentRepository.Remove(cachedContent);
            }

            _logger.LogDebug("Content not in cache: {ExternalId}", externalId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting content from cache");
            return null;
        }
    }

    public async Task<ContentItem> CacheContentAsync(
        ContentItem content,
        int cacheDurationHours)
    {
        try
        {
            // Verificar si ya existe en caché
            var existing = await _contentRepository.FindByExternalIdAsync(content.ExternalId);

            if (existing != null)
            {
                // Actualizar contenido existente
                existing.UpdateMetadata(content.Metadata, cacheDurationHours);
                _contentRepository.Update(existing);
                _logger.LogDebug("Content updated in cache: {ExternalId}", content.ExternalId);
                return existing;
            }
            else
            {
                // Agregar nuevo contenido
                await _contentRepository.AddAsync(content);
                _logger.LogDebug("Content added to cache: {ExternalId}", content.ExternalId);
                return content;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching content");
            return content;
        }
    }

    public async Task<bool> IsCachedAndValidAsync(string externalId)
    {
        try
        {
            var content = await _contentRepository.FindByExternalIdAsync(externalId);
            return content != null && !content.IsExpired();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache validity");
            return false;
        }
    }

    public async Task InvalidateCacheAsync(string externalId)
    {
        try
        {
            var content = await _contentRepository.FindByExternalIdAsync(externalId);
            if (content != null)
            {
                _contentRepository.Remove(content);
                _logger.LogDebug("Cache invalidated for: {ExternalId}", externalId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache");
        }
    }

    public int GetCacheDurationForType(ContentType contentType)
    {
        return contentType switch
        {
            ContentType.Place => _cacheSettings.PlaceCacheDurationHours,
            ContentType.Movie => _cacheSettings.ContentCacheDurationHours,
            ContentType.Series => _cacheSettings.ContentCacheDurationHours,
            ContentType.Music => _cacheSettings.ContentCacheDurationHours,
            ContentType.Video => _cacheSettings.ContentCacheDurationHours,
            _ => _cacheSettings.ContentCacheDurationHours
        };
    }

    public async Task<int> CleanExpiredCacheAsync()
    {
        try
        {
            var removed = await _contentRepository.RemoveExpiredAsync();
            _logger.LogInformation("Cleaned {Count} expired items from cache", removed);
            return removed;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning expired cache");
            return 0;
        }
    }
}
