using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftFocusBackend.Library.Domain.Model.ValueObjects;
using SoftFocusBackend.Library.Infrastructure.ExternalServices.OpenWeather.Configuration;

namespace SoftFocusBackend.Library.Infrastructure.ExternalServices.OpenWeather.Services;

/// <summary>
/// Implementación del servicio para interactuar con la API de OpenWeather
/// </summary>
public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly OpenWeatherSettings _settings;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(
        HttpClient httpClient,
        IOptions<OpenWeatherSettings> settings,
        ILogger<WeatherService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<WeatherCondition?> GetCurrentWeatherAsync(double latitude, double longitude)
    {
        try
        {
            var url = _settings.GetWeatherUrl(latitude, longitude);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("OpenWeather API error: {StatusCode}", response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<OpenWeatherResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result?.Weather == null || !result.Weather.Any())
                return null;

            var weather = result.Weather.First();

            return WeatherCondition.Create(
                condition: weather.Main ?? "Unknown",
                description: weather.Description ?? string.Empty,
                temperature: result.Main?.Temp ?? 0,
                humidity: result.Main?.Humidity ?? 0,
                cityName: result.Name ?? "Unknown"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting weather from OpenWeather");
            return null;
        }
    }

    // DTOs para deserialización
    private class OpenWeatherResponse
    {
        public List<WeatherData>? Weather { get; set; }
        public MainData? Main { get; set; }
        public string? Name { get; set; }
    }

    private class WeatherData
    {
        public string? Main { get; set; }
        public string? Description { get; set; }
    }

    private class MainData
    {
        public double Temp { get; set; }
        public int Humidity { get; set; }
    }
}
