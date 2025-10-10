namespace SoftFocusBackend.Library.Infrastructure.ExternalServices.Spotify.Configuration;

/// <summary>
/// Configuración para la API de Spotify
/// </summary>
public class SpotifySettings
{
    /// <summary>
    /// Client ID de la aplicación Spotify (cargado desde variables de entorno)
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Client Secret de la aplicación Spotify (cargado desde variables de entorno)
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// URL base de la API de Spotify
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.spotify.com/v1";

    /// <summary>
    /// URL para autenticación (Client Credentials Flow)
    /// </summary>
    public string AuthUrl { get; set; } = "https://accounts.spotify.com/api/token";
}
