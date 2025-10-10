using Microsoft.Extensions.Logging;
using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Model.ValueObjects;
using SoftFocusBackend.Library.Domain.Services;
using SoftFocusBackend.Library.Infrastructure.ExternalServices.Foursquare.Services;
using SoftFocusBackend.Library.Infrastructure.ExternalServices.OpenWeather.Services;

namespace SoftFocusBackend.Library.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de recomendación de lugares basado en clima y emoción
/// </summary>
public class WeatherPlaceRecommenderService : IWeatherPlaceRecommender
{
    private readonly IWeatherService _weatherService;
    private readonly IFoursquareService _foursquareService;
    private readonly ILogger<WeatherPlaceRecommenderService> _logger;

    // Mapeo de condiciones climáticas a categorías de lugares
    private static readonly Dictionary<string, string[]> WeatherToCategories = new()
    {
        { "Clear", new[] { "park", "outdoor", "plaza", "beach" } },
        { "Clouds", new[] { "cafe", "museum", "library", "mall" } },
        { "Rain", new[] { "cafe", "cinema", "museum", "mall", "library" } },
        { "Drizzle", new[] { "cafe", "museum", "library" } },
        { "Thunderstorm", new[] { "cafe", "cinema", "mall" } },
        { "Snow", new[] { "cafe", "museum", "cinema" } }
    };

    // Mapeo de emociones a categorías de lugares
    private static readonly Dictionary<EmotionalTag, string[]> EmotionToCategories = new()
    {
        { EmotionalTag.Calm, new[] { "park", "library", "museum", "plaza" } },
        { EmotionalTag.Energetic, new[] { "outdoor", "park", "beach" } },
        { EmotionalTag.Happy, new[] { "cinema", "cafe", "plaza", "park" } },
        { EmotionalTag.Sad, new[] { "cafe", "library", "museum" } },
        { EmotionalTag.Anxious, new[] { "park", "library", "cafe" } }
    };

    // Mapeo de categorías a tags emocionales
    private static readonly Dictionary<string, EmotionalTag> CategoryToEmotion = new()
    {
        { "park", EmotionalTag.Calm },
        { "library", EmotionalTag.Calm },
        { "museum", EmotionalTag.Calm },
        { "cafe", EmotionalTag.Calm },
        { "beach", EmotionalTag.Calm },
        { "plaza", EmotionalTag.Happy },
        { "cinema", EmotionalTag.Happy },
        { "outdoor", EmotionalTag.Energetic },
        { "mall", EmotionalTag.Happy }
    };

    public WeatherPlaceRecommenderService(
        IWeatherService weatherService,
        IFoursquareService foursquareService,
        ILogger<WeatherPlaceRecommenderService> logger)
    {
        _weatherService = weatherService;
        _foursquareService = foursquareService;
        _logger = logger;
    }

    public async Task<WeatherCondition> GetCurrentWeatherAsync(double latitude, double longitude)
    {
        try
        {
            var weather = await _weatherService.GetCurrentWeatherAsync(latitude, longitude);

            if (weather == null)
            {
                _logger.LogWarning("Could not retrieve weather data");
                // Retornar condición por defecto
                return WeatherCondition.Create("Clouds", "Nublado", 20, 50, "Unknown");
            }

            return weather;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting weather");
            return WeatherCondition.Create("Clouds", "Nublado", 20, 50, "Unknown");
        }
    }

    public async Task<List<ContentItem>> RecommendPlacesAsync(
        double latitude,
        double longitude,
        EmotionalTag? emotion = null,
        int radius = 5000,
        int limit = 10)
    {
        try
        {
            // Obtener clima actual
            var weather = await GetCurrentWeatherAsync(latitude, longitude);

            // Determinar categorías basadas en clima
            var weatherCategories = GetPlaceCategoriesForWeather(weather.Condition);

            // Si hay emoción, combinar con categorías de emoción
            List<string> finalCategories;
            if (emotion.HasValue)
            {
                var emotionCategories = GetPlaceCategoriesForEmotion(emotion.Value);
                finalCategories = CombineCategories(weatherCategories, emotionCategories);
            }
            else
            {
                finalCategories = weatherCategories;
            }

            // Buscar lugares en Foursquare
            var places = await _foursquareService.SearchPlacesAsync(
                latitude,
                longitude,
                finalCategories,
                radius,
                limit
            );

            // Agregar tag emocional apropiado a cada lugar
            foreach (var place in places)
            {
                var category = place.Metadata.Category.ToLower();
                var emotionalTag = GetEmotionalTagForPlaceCategory(category);

                if (!place.EmotionalTags.Contains(emotionalTag))
                {
                    place.AddEmotionalTag(emotionalTag);
                }
            }

            return places;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recommending places");
            return new List<ContentItem>();
        }
    }

    public List<string> GetPlaceCategoriesForWeather(string weatherCondition)
    {
        return WeatherToCategories.ContainsKey(weatherCondition)
            ? WeatherToCategories[weatherCondition].ToList()
            : WeatherToCategories["Clouds"].ToList(); // Default: indoor
    }

    public List<string> GetPlaceCategoriesForEmotion(EmotionalTag emotion)
    {
        return EmotionToCategories.ContainsKey(emotion)
            ? EmotionToCategories[emotion].ToList()
            : new List<string> { "park", "cafe" };
    }

    public List<string> CombineCategories(
        List<string> weatherCategories,
        List<string>? emotionCategories)
    {
        if (emotionCategories == null || !emotionCategories.Any())
            return weatherCategories;

        // Intersección: priorizar categorías que estén en ambas listas
        var intersection = weatherCategories.Intersect(emotionCategories).ToList();

        if (intersection.Any())
            return intersection;

        // Si no hay intersección, combinar y dar preferencia a clima
        var combined = weatherCategories.Concat(emotionCategories).Distinct().ToList();
        return combined.Take(3).ToList();
    }

    public EmotionalTag GetEmotionalTagForPlaceCategory(string category)
    {
        var lowerCategory = category.ToLower();

        foreach (var (cat, emotion) in CategoryToEmotion)
        {
            if (lowerCategory.Contains(cat))
                return emotion;
        }

        // Default
        return EmotionalTag.Calm;
    }
}
