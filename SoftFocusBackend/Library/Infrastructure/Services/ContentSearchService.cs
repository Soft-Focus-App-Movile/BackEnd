using Microsoft.Extensions.Logging;
using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Model.ValueObjects;
using SoftFocusBackend.Library.Domain.Repositories;
using SoftFocusBackend.Library.Domain.Services;
using SoftFocusBackend.Library.Infrastructure.ExternalServices.TMDB.Services;
using SoftFocusBackend.Library.Infrastructure.ExternalServices.Spotify.Services;
using SoftFocusBackend.Library.Infrastructure.ExternalServices.YouTube.Services;
using SoftFocusBackend.Shared.Domain.Repositories;

namespace SoftFocusBackend.Library.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de búsqueda de contenido
/// Coordina las búsquedas en APIs externas (TMDB, Spotify, YouTube)
/// </summary>
public class ContentSearchService : IContentSearchService
{
    private readonly ITMDBService _tmdbService;
    private readonly ISpotifyService _spotifyService;
    private readonly IYouTubeService _youtubeService;
    private readonly IEmotionContentMatcher _emotionMatcher;
    private readonly IContentItemRepository _contentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ContentSearchService> _logger;

    public ContentSearchService(
        ITMDBService tmdbService,
        ISpotifyService spotifyService,
        IYouTubeService youtubeService,
        IEmotionContentMatcher emotionMatcher,
        IContentItemRepository contentRepository,
        IUnitOfWork unitOfWork,
        ILogger<ContentSearchService> logger)
    {
        _tmdbService = tmdbService;
        _spotifyService = spotifyService;
        _youtubeService = youtubeService;
        _emotionMatcher = emotionMatcher;
        _contentRepository = contentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<ContentItem>> SearchMoviesAsync(
        string query,
        EmotionalTag? emotionFilter = null,
        int limit = 20)
    {
        try
        {
            List<ContentItem> results;

            if (emotionFilter.HasValue)
            {
                // Buscar con filtro de géneros según emoción
                var genreIds = _emotionMatcher.GetMovieGenresForEmotion(emotionFilter.Value);
                results = await _tmdbService.SearchMoviesByGenresAsync(query, genreIds, limit);
            }
            else
            {
                results = await _tmdbService.SearchMoviesAsync(query, limit);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching movies");
            return new List<ContentItem>();
        }
    }

    public async Task<List<ContentItem>> SearchSeriesAsync(
        string query,
        EmotionalTag? emotionFilter = null,
        int limit = 20)
    {
        try
        {
            var results = await _tmdbService.SearchSeriesAsync(query, limit);

            // Filtrar por emoción si se especificó
            if (emotionFilter.HasValue)
            {
                results = results
                    .Where(s => s.EmotionalTags.Contains(emotionFilter.Value))
                    .Take(limit)
                    .ToList();
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching series");
            return new List<ContentItem>();
        }
    }

    public async Task<List<ContentItem>> SearchMusicAsync(
        string query,
        EmotionalTag? emotionFilter = null,
        int limit = 20)
    {
        try
        {
            List<ContentItem> results;

            if (emotionFilter.HasValue)
            {
                // Usar query especializado para emociones
                var emotionQuery = _emotionMatcher.GetSpotifyQueryForEmotion(emotionFilter.Value);
                var combinedQuery = $"{query} {emotionQuery}";
                _logger.LogInformation("ContentSearchService: Spotify combined query: '{CombinedQuery}'", combinedQuery);
                results = await _spotifyService.SearchTracksAsync(combinedQuery, limit);
            }
            else
            {
                _logger.LogInformation("ContentSearchService: Spotify query: '{Query}'", query);
                results = await _spotifyService.SearchTracksAsync(query, limit);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching music");
            return new List<ContentItem>();
        }
    }

    public async Task<List<ContentItem>> SearchVideosAsync(
        string query,
        EmotionalTag? emotionFilter = null,
        int limit = 20)
    {
        try
        {
            List<ContentItem> results;

            if (emotionFilter.HasValue)
            {
                // Usar query especializado para emociones
                var emotionQuery = _emotionMatcher.GetYouTubeQueryForEmotion(emotionFilter.Value);
                var combinedQuery = $"{query} {emotionQuery}";
                results = await _youtubeService.SearchVideosAsync(combinedQuery, limit);
            }
            else
            {
                results = await _youtubeService.SearchVideosAsync(query, limit);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching videos");
            return new List<ContentItem>();
        }
    }

    public async Task<List<ContentItem>> SearchContentAsync(
        string query,
        ContentType contentType,
        EmotionalTag? emotionFilter = null,
        int limit = 20)
    {
        var results = contentType switch
        {
            ContentType.Movie => await SearchMoviesAsync(query, emotionFilter, limit),
            ContentType.Series => await SearchSeriesAsync(query, emotionFilter, limit),
            ContentType.Music => await SearchMusicAsync(query, emotionFilter, limit),
            ContentType.Video => await SearchVideosAsync(query, emotionFilter, limit),
            _ => new List<ContentItem>()
        };

        // Guardar los resultados en la base de datos para que puedan ser agregados a favoritos
        await SaveContentItemsAsync(results);

        return results;
    }

    private async Task SaveContentItemsAsync(List<ContentItem> items)
    {
        try
        {
            foreach (var item in items)
            {
                // Verificar si ya existe en la base de datos
                var existingItem = await _contentRepository.FindByExternalIdAsync(item.ExternalId);

                if (existingItem == null)
                {
                    // No existe, agregarlo
                    await _contentRepository.AddAsync(item);
                    _logger.LogInformation("Content item saved to database: {ExternalId}", item.ExternalId);
                }
                else
                {
                    // Ya existe, actualizar la fecha de último acceso
                    existingItem.RefreshCache();
                    _contentRepository.Update(existingItem);
                    _logger.LogDebug("Content item already exists, updated cache: {ExternalId}", item.ExternalId);
                }
            }

            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("Saved {Count} content items to database", items.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving content items to database");
            // No lanzamos excepción para no afectar la búsqueda
        }
    }
}
