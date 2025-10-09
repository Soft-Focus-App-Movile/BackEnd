using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Library.Domain.Services;

/// <summary>
/// Servicio de dominio para emparejar contenido con emociones
/// </summary>
public interface IEmotionContentMatcher
{
    /// <summary>
    /// Obtiene contenido recomendado según una emoción
    /// </summary>
    Task<List<ContentItem>> GetContentForEmotionAsync(
        EmotionalTag emotion,
        ContentType? contentType = null,
        int limit = 20);

    /// <summary>
    /// Determina qué tags emocionales aplicar a una película basándose en géneros
    /// </summary>
    List<EmotionalTag> MapMovieGenresToEmotions(List<int> genreIds);

    /// <summary>
    /// Determina qué tags emocionales aplicar a música basándose en características
    /// </summary>
    List<EmotionalTag> MapMusicToEmotions(string trackName, string artistName);

    /// <summary>
    /// Determina qué tags emocionales aplicar a un video basándose en título/descripción
    /// </summary>
    List<EmotionalTag> MapVideoToEmotions(string title, string description);

    /// <summary>
    /// Obtiene géneros de TMDB que corresponden a una emoción
    /// </summary>
    List<int> GetMovieGenresForEmotion(EmotionalTag emotion);

    /// <summary>
    /// Obtiene query de búsqueda de Spotify para una emoción
    /// </summary>
    string GetSpotifyQueryForEmotion(EmotionalTag emotion);

    /// <summary>
    /// Obtiene query de búsqueda de YouTube para una emoción
    /// </summary>
    string GetYouTubeQueryForEmotion(EmotionalTag emotion);
}
