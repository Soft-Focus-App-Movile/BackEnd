using SoftFocusBackend.Library.Domain.Model.Aggregates;

namespace SoftFocusBackend.Library.Infrastructure.ExternalServices.TMDB.Services;

/// <summary>
/// Servicio para interactuar con la API de TMDB
/// </summary>
public interface ITMDBService
{
    /// <summary>
    /// Busca películas por término de búsqueda
    /// </summary>
    Task<List<ContentItem>> SearchMoviesAsync(string query, int limit = 20);

    /// <summary>
    /// Busca series por término de búsqueda
    /// </summary>
    Task<List<ContentItem>> SearchSeriesAsync(string query, int limit = 20);

    /// <summary>
    /// Busca películas filtradas por géneros
    /// </summary>
    Task<List<ContentItem>> SearchMoviesByGenresAsync(
        string query,
        List<int> genreIds,
        int limit = 20);

    /// <summary>
    /// Obtiene detalles de una película específica
    /// </summary>
    Task<ContentItem?> GetMovieDetailsAsync(int movieId);

    /// <summary>
    /// Obtiene detalles de una serie específica
    /// </summary>
    Task<ContentItem?> GetSeriesDetailsAsync(int seriesId);
}
