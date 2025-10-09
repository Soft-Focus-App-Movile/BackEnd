using SoftFocusBackend.Library.Domain.Model.Aggregates;

namespace SoftFocusBackend.Library.Infrastructure.ExternalServices.YouTube.Services;

/// <summary>
/// Servicio para interactuar con la API de YouTube Data v3
/// </summary>
public interface IYouTubeService
{
    /// <summary>
    /// Busca videos por término de búsqueda
    /// </summary>
    Task<List<ContentItem>> SearchVideosAsync(string query, int limit = 20);

    /// <summary>
    /// Busca videos usando un query específico para emociones
    /// </summary>
    Task<List<ContentItem>> SearchVideosByEmotionQueryAsync(
        string emotionQuery,
        int limit = 20);

    /// <summary>
    /// Obtiene detalles de un video específico
    /// </summary>
    Task<ContentItem?> GetVideoDetailsAsync(string videoId);
}
