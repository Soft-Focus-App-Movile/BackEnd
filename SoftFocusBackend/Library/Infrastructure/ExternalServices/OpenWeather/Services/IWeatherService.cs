using SoftFocusBackend.Library.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Library.Infrastructure.ExternalServices.OpenWeather.Services;

/// <summary>
/// Servicio para interactuar con la API de OpenWeather
/// </summary>
public interface IWeatherService
{
    /// <summary>
    /// Obtiene el clima actual en una ubicación específica
    /// </summary>
    Task<WeatherCondition?> GetCurrentWeatherAsync(double latitude, double longitude);
}
