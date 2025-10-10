using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Library.Domain.Services;

/// <summary>
/// Servicio de dominio para búsqueda de contenido en APIs externas
/// </summary>
public interface IContentSearchService
{
    /// <summary>
    /// Busca películas en TMDB
    /// </summary>
    Task<List<ContentItem>> SearchMoviesAsync(
        string query,
        EmotionalTag? emotionFilter = null,
        int limit = 20);

    /// <summary>
    /// Busca series en TMDB
    /// </summary>
    Task<List<ContentItem>> SearchSeriesAsync(
        string query,
        EmotionalTag? emotionFilter = null,
        int limit = 20);

    /// <summary>
    /// Busca música en Spotify
    /// </summary>
    Task<List<ContentItem>> SearchMusicAsync(
        string query,
        EmotionalTag? emotionFilter = null,
        int limit = 20);

    /// <summary>
    /// Busca videos de bienestar en YouTube
    /// </summary>
    Task<List<ContentItem>> SearchVideosAsync(
        string query,
        EmotionalTag? emotionFilter = null,
        int limit = 20);

    /// <summary>
    /// Busca contenido según tipo (delegando a método específico)
    /// </summary>
    Task<List<ContentItem>> SearchContentAsync(
        string query,
        ContentType contentType,
        EmotionalTag? emotionFilter = null,
        int limit = 20);
}
