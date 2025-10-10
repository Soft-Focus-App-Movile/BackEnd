namespace SoftFocusBackend.Library.Infrastructure.Configuration;

/// <summary>
/// Configuración para el sistema de caché de contenidos
/// </summary>
public class LibraryCacheSettings
{
    /// <summary>
    /// Duración del caché para contenido multimedia (películas, series, música, videos) en horas
    /// </summary>
    public int ContentCacheDurationHours { get; set; } = 24;

    /// <summary>
    /// Duración del caché para lugares en horas
    /// </summary>
    public int PlaceCacheDurationHours { get; set; } = 6;

    /// <summary>
    /// Duración del caché para búsquedas en minutos
    /// </summary>
    public int SearchCacheDurationMinutes { get; set; } = 30;

    /// <summary>
    /// Obtiene la duración de caché en horas según el tipo de contenido
    /// </summary>
    public int GetCacheDurationHours(string contentType)
    {
        return contentType.ToLower() switch
        {
            "place" => PlaceCacheDurationHours,
            "movie" => ContentCacheDurationHours,
            "series" => ContentCacheDurationHours,
            "music" => ContentCacheDurationHours,
            "video" => ContentCacheDurationHours,
            _ => ContentCacheDurationHours
        };
    }
}
