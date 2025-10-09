namespace SoftFocusBackend.Library.Infrastructure.ExternalServices.YouTube.Configuration;

/// <summary>
/// Configuración para la API de YouTube Data v3
/// </summary>
public class YouTubeSettings
{
    /// <summary>
    /// API Key de YouTube (cargada desde variables de entorno)
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// URL base de la API de YouTube
    /// </summary>
    public string BaseUrl { get; set; } = "https://www.googleapis.com/youtube/v3";

    /// <summary>
    /// Número máximo de resultados por búsqueda
    /// </summary>
    public int MaxResults { get; set; } = 20;

    /// <summary>
    /// Duración de videos a buscar (medium = 4-20 min)
    /// </summary>
    public string VideoDuration { get; set; } = "medium";

    /// <summary>
    /// Construye URL completa de un video de YouTube
    /// </summary>
    public string GetVideoUrl(string videoId)
    {
        if (string.IsNullOrEmpty(videoId))
            return string.Empty;

        return $"https://www.youtube.com/watch?v={videoId}";
    }

    /// <summary>
    /// Construye URL de thumbnail de un video
    /// </summary>
    public string GetThumbnailUrl(string videoId, string quality = "hqdefault")
    {
        if (string.IsNullOrEmpty(videoId))
            return string.Empty;

        return $"https://i.ytimg.com/vi/{videoId}/{quality}.jpg";
    }
}
