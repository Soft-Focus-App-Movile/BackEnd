namespace SoftFocusBackend.Library.Infrastructure.ExternalServices.Foursquare.Configuration;

/// <summary>
/// Configuración para la API de Foursquare Places
/// </summary>
public class FoursquareSettings
{
    /// <summary>
    /// API Key de Foursquare (cargada desde variables de entorno)
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// URL base de la API de Foursquare Places
    /// </summary>
    public string BaseUrl { get; set; } = "https://places-api.foursquare.com";

    /// <summary>
    /// Versión de la API de Foursquare Places
    /// </summary>
    public string ApiVersion { get; set; } = "2025-06-17";

    /// <summary>
    /// Radio de búsqueda por defecto en metros
    /// </summary>
    public int DefaultRadius { get; set; } = 5000;

    /// <summary>
    /// Número máximo de resultados por búsqueda
    /// </summary>
    public int MaxResults { get; set; } = 10;

    /// <summary>
    /// URL de imagen placeholder para lugares sin foto
    /// </summary>
    public string DefaultPlaceholderPhotoUrl { get; set; } = "https://via.placeholder.com/300x300/2D3748/718096?text=Lugar";

    /// <summary>
    /// Construye URL de foto de un lugar
    /// </summary>
    public string GetPhotoUrl(string prefix, string suffix, string size = "300x300")
    {
        if (string.IsNullOrEmpty(prefix) || string.IsNullOrEmpty(suffix))
            return DefaultPlaceholderPhotoUrl;

        return $"{prefix}{size}{suffix}";
    }
}
