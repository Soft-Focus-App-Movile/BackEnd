namespace SoftFocusBackend.Library.Infrastructure.ExternalServices.OpenWeather.Configuration;

/// <summary>
/// Configuración para la API de OpenWeather
/// </summary>
public class OpenWeatherSettings
{
    /// <summary>
    /// API Key de OpenWeather (cargada desde variables de entorno)
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// URL base de la API de OpenWeather
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.openweathermap.org/data/2.5";

    /// <summary>
    /// Unidades de medida (metric = Celsius, imperial = Fahrenheit)
    /// </summary>
    public string Units { get; set; } = "metric";

    /// <summary>
    /// Idioma para descripciones del clima
    /// </summary>
    public string Language { get; set; } = "es";

    /// <summary>
    /// Construye URL completa para obtener clima actual
    /// </summary>
    public string GetWeatherUrl(double latitude, double longitude)
    {
        return $"{BaseUrl}/weather?lat={latitude}&lon={longitude}&appid={ApiKey}&units={Units}&lang={Language}";
    }
}
