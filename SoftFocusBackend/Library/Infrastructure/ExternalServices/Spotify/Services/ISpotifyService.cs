using SoftFocusBackend.Library.Domain.Model.Aggregates;

namespace SoftFocusBackend.Library.Infrastructure.ExternalServices.Spotify.Services;

/// <summary>
/// Servicio para interactuar con la API de Spotify
/// </summary>
public interface ISpotifyService
{
    /// <summary>
    /// Busca canciones por término de búsqueda
    /// </summary>
    Task<List<ContentItem>> SearchTracksAsync(string query, int limit = 20);

    /// <summary>
    /// Busca canciones usando un query específico para emociones
    /// </summary>
    Task<List<ContentItem>> SearchTracksByEmotionQueryAsync(
        string emotionQuery,
        int limit = 20);

    /// <summary>
    /// Obtiene detalles de una canción específica
    /// </summary>
    Task<ContentItem?> GetTrackDetailsAsync(string trackId);

    /// <summary>
    /// Autentica y obtiene un access token (Client Credentials Flow)
    /// </summary>
    Task<string?> GetAccessTokenAsync();

    Task<List<ContentItem>> GetPopularTracksAsync(int limit = 20);
}
