namespace SoftFocusBackend.Library.Infrastructure.ExternalServices.TMDB.Configuration;

/// <summary>
/// Configuración para la API de TMDB (The Movie Database)
/// </summary>
public class TMDBSettings
{
    /// <summary>
    /// API Key de TMDB (cargada desde variables de entorno)
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// URL base de la API de TMDB
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.themoviedb.org/3";

    /// <summary>
    /// Idioma para resultados (español de Perú)
    /// </summary>
    public string Language { get; set; } = "es-PE";

    /// <summary>
    /// URL base para imágenes
    /// </summary>
    public string ImageBaseUrl { get; set; } = "https://image.tmdb.org/t/p";

    /// <summary>
    /// Construye URL completa para un poster
    /// </summary>
    public string GetPosterUrl(string posterPath, string size = "w500")
    {
        if (string.IsNullOrEmpty(posterPath))
            return string.Empty;

        return $"{ImageBaseUrl}/{size}{posterPath}";
    }

    /// <summary>
    /// Construye URL completa para un backdrop
    /// </summary>
    public string GetBackdropUrl(string backdropPath, string size = "w1280")
    {
        if (string.IsNullOrEmpty(backdropPath))
            return string.Empty;

        return $"{ImageBaseUrl}/{size}{backdropPath}";
    }

    /// <summary>
    /// Construye URL de YouTube para un trailer
    /// </summary>
    public string GetTrailerUrl(string videoKey)
    {
        if (string.IsNullOrEmpty(videoKey))
            return string.Empty;

        return $"https://www.youtube.com/watch?v={videoKey}";
    }
}
