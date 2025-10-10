using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Library.Domain.Services;

/// <summary>
/// Servicio de dominio para recomendar lugares basado en clima y emoción
/// </summary>
public interface IWeatherPlaceRecommender
{
    /// <summary>
    /// Obtiene el clima actual en una ubicación
    /// </summary>
    Task<WeatherCondition> GetCurrentWeatherAsync(double latitude, double longitude);

    /// <summary>
    /// Recomienda lugares basándose en clima y emoción
    /// </summary>
    Task<List<ContentItem>> RecommendPlacesAsync(
        double latitude,
        double longitude,
        EmotionalTag? emotion = null,
        int radius = 5000,
        int limit = 10);

    /// <summary>
    /// Determina categorías de lugares según condición climática
    /// </summary>
    List<string> GetPlaceCategoriesForWeather(string weatherCondition);

    /// <summary>
    /// Determina categorías de lugares según emoción
    /// </summary>
    List<string> GetPlaceCategoriesForEmotion(EmotionalTag emotion);

    /// <summary>
    /// Combina categorías de clima y emoción para refinar recomendaciones
    /// </summary>
    List<string> CombineCategories(
        List<string> weatherCategories,
        List<string>? emotionCategories);

    /// <summary>
    /// Determina el tag emocional que mejor representa una categoría de lugar
    /// </summary>
    EmotionalTag GetEmotionalTagForPlaceCategory(string category);
}
