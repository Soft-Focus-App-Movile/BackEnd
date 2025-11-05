using Microsoft.Extensions.Logging;
using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Model.ValueObjects;
using SoftFocusBackend.Library.Domain.Repositories;
using SoftFocusBackend.Library.Domain.Services;

namespace SoftFocusBackend.Library.Infrastructure.Services;

/// <summary>
/// Implementación del servicio de emparejamiento de contenido con emociones
/// Contiene toda la lógica de mapeo entre emociones y contenido
/// </summary>
public class EmotionContentMatcherService : IEmotionContentMatcher
{
    private readonly IContentItemRepository _contentRepository;
    private readonly ICachePopulationService _cachePopulationService;
    private readonly ILogger<EmotionContentMatcherService> _logger;

    private static readonly Dictionary<EmotionalTag, int[]> EmotionToMovieGenres = new()
    {
        { EmotionalTag.Happy, new[] { 35, 10751, 10749 } },
        { EmotionalTag.Energetic, new[] { 28, 12, 878 } },
        { EmotionalTag.Calm, new[] { 18, 99, 36 } },
        { EmotionalTag.Sad, new[] { 18, 10749 } },
        { EmotionalTag.Anxious, new[] { 10751, 16, 10402 } }
    };

    private static readonly Dictionary<EmotionalTag, string> EmotionToSpotifyQuery = new()
    {
        { EmotionalTag.Happy, "happy upbeat positive cheerful" },
        { EmotionalTag.Calm, "calm relaxing peaceful meditation ambient" },
        { EmotionalTag.Energetic, "energetic workout motivation upbeat" },
        { EmotionalTag.Sad, "sad melancholic emotional ballad" },
        { EmotionalTag.Anxious, "calming anxiety relief stress soothing" }
    };

    private static readonly Dictionary<EmotionalTag, string> EmotionToYouTubeQuery = new()
    {
        { EmotionalTag.Anxious, "ejercicios respiración ansiedad meditación guiada" },
        { EmotionalTag.Calm, "meditación guiada relajación mindfulness" },
        { EmotionalTag.Energetic, "yoga energizante ejercicios motivación" },
        { EmotionalTag.Sad, "meditación sanación emocional autoestima" },
        { EmotionalTag.Happy, "yoga alegría energía positiva bienestar" }
    };

    public EmotionContentMatcherService(
        IContentItemRepository contentRepository,
        ICachePopulationService cachePopulationService,
        ILogger<EmotionContentMatcherService> logger)
    {
        _contentRepository = contentRepository;
        _cachePopulationService = cachePopulationService;
        _logger = logger;
    }

    public async Task<List<ContentItem>> GetContentForEmotionAsync(
        EmotionalTag emotion,
        ContentType? contentType = null,
        int limit = 20)
    {
        try
        {
            if (contentType.HasValue)
            {
                var results = await _contentRepository.FindByTypeAndEmotionAsync(
                    contentType.Value,
                    emotion,
                    limit
                );

                if (!results.Any())
                {
                    _logger.LogInformation(
                        "No content found in cache for emotion {Emotion} and type {Type}, populating from APIs",
                        emotion, contentType);

                    var populated = await _cachePopulationService.PopulateCacheForTypeAsync(
                        contentType.Value,
                        emotion,
                        limit
                    );

                    results = populated;
                }

                return results.ToList();
            }
            else
            {
                var results = new List<ContentItem>();

                foreach (ContentType type in Enum.GetValues(typeof(ContentType)))
                {
                    if (type == ContentType.Place) continue;

                    var typeResults = await _contentRepository.FindByTypeAndEmotionAsync(
                        type,
                        emotion,
                        limit / 4
                    );
                    results.AddRange(typeResults);
                }

                if (!results.Any())
                {
                    _logger.LogInformation(
                        "No content found in cache for emotion {Emotion}, populating from APIs",
                        emotion);

                    foreach (ContentType type in Enum.GetValues(typeof(ContentType)))
                    {
                        if (type == ContentType.Place) continue;

                        var populated = await _cachePopulationService.PopulateCacheForTypeAsync(
                            type,
                            emotion,
                            limit / 4
                        );
                        results.AddRange(populated);
                    }
                }

                return results.Take(limit).ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting content for emotion");
            return new List<ContentItem>();
        }
    }

    public List<EmotionalTag> MapMovieGenresToEmotions(List<int> genreIds)
    {
        var tags = new List<EmotionalTag>();

        foreach (var (emotion, genres) in EmotionToMovieGenres)
        {
            if (genreIds.Any(id => genres.Contains(id)))
            {
                tags.Add(emotion);
            }
        }

        return tags.Distinct().ToList();
    }

    public List<EmotionalTag> MapMusicToEmotions(string trackName, string artistName)
    {
        var tags = new List<EmotionalTag>();
        var combined = $"{trackName} {artistName}".ToLower();

        // Mapeo por palabras clave
        if (combined.Contains("happy") || combined.Contains("alegre") || combined.Contains("feliz") ||
            combined.Contains("joy"))
            tags.Add(EmotionalTag.Happy);

        if (combined.Contains("calm") || combined.Contains("relax") || combined.Contains("peace") ||
            combined.Contains("meditation") || combined.Contains("ambient"))
            tags.Add(EmotionalTag.Calm);

        if (combined.Contains("energy") || combined.Contains("workout") || combined.Contains("motivat") ||
            combined.Contains("upbeat") || combined.Contains("pump"))
            tags.Add(EmotionalTag.Energetic);

        if (combined.Contains("sad") || combined.Contains("melanchol") || combined.Contains("triste") ||
            combined.Contains("emotional") || combined.Contains("ballad"))
            tags.Add(EmotionalTag.Sad);

        if (combined.Contains("anxiety") || combined.Contains("stress") || combined.Contains("sooth"))
            tags.Add(EmotionalTag.Anxious);

        // Si no se detectó emoción, agregar Calm por defecto
        if (!tags.Any())
            tags.Add(EmotionalTag.Calm);

        return tags;
    }

    public List<EmotionalTag> MapVideoToEmotions(string title, string description)
    {
        var tags = new List<EmotionalTag>();
        var combined = $"{title} {description}".ToLower();

        // Mapeo por palabras clave en español e inglés
        if (combined.Contains("ansiedad") || combined.Contains("anxiety") || combined.Contains("estrés") ||
            combined.Contains("stress") || combined.Contains("nervios"))
            tags.Add(EmotionalTag.Anxious);

        if (combined.Contains("meditación") || combined.Contains("meditation") || combined.Contains("relajación") ||
            combined.Contains("relax") || combined.Contains("calm") || combined.Contains("mindfulness") ||
            combined.Contains("paz"))
            tags.Add(EmotionalTag.Calm);

        if (combined.Contains("energía") || combined.Contains("energy") || combined.Contains("yoga") ||
            combined.Contains("ejercicio") || combined.Contains("activación") || combined.Contains("motivación"))
            tags.Add(EmotionalTag.Energetic);

        if (combined.Contains("tristeza") || combined.Contains("sad") || combined.Contains("sanación") ||
            combined.Contains("duelo") || combined.Contains("grief") || combined.Contains("emotional"))
            tags.Add(EmotionalTag.Sad);

        if (combined.Contains("feliz") || combined.Contains("happy") || combined.Contains("alegría") ||
            combined.Contains("joy") || combined.Contains("positiv"))
            tags.Add(EmotionalTag.Happy);

        // Videos de bienestar tienden a ser calmantes por defecto
        if (!tags.Any())
            tags.Add(EmotionalTag.Calm);

        return tags;
    }

    public List<int> GetMovieGenresForEmotion(EmotionalTag emotion)
    {
        return EmotionToMovieGenres.ContainsKey(emotion)
            ? EmotionToMovieGenres[emotion].ToList()
            : new List<int>();
    }

    public string GetSpotifyQueryForEmotion(EmotionalTag emotion)
    {
        return EmotionToSpotifyQuery.ContainsKey(emotion)
            ? EmotionToSpotifyQuery[emotion]
            : "relaxing music";
    }

    public string GetYouTubeQueryForEmotion(EmotionalTag emotion)
    {
        return EmotionToYouTubeQuery.ContainsKey(emotion)
            ? EmotionToYouTubeQuery[emotion]
            : "meditación bienestar";
    }
}
