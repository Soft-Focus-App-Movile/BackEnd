using Microsoft.Extensions.Logging;
using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Model.ValueObjects;
using SoftFocusBackend.Library.Domain.Repositories;
using SoftFocusBackend.Library.Domain.Services;
using SoftFocusBackend.Library.Infrastructure.ExternalServices.TMDB.Services;
using SoftFocusBackend.Library.Infrastructure.ExternalServices.Spotify.Services;
using SoftFocusBackend.Library.Infrastructure.ExternalServices.YouTube.Services;
using SoftFocusBackend.Shared.Domain.Repositories;

namespace SoftFocusBackend.Library.Infrastructure.Services;

public class CachePopulationService : ICachePopulationService
{
    private readonly ITMDBService _tmdbService;
    private readonly ISpotifyService _spotifyService;
    private readonly IYouTubeService _youtubeService;
    private readonly IContentItemRepository _contentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CachePopulationService> _logger;

    private static readonly int[] RestrictedGenresForMentalHealth = new[]
    {
        27, 53, 80, 10752, 9648
    };

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

    public CachePopulationService(
        ITMDBService tmdbService,
        ISpotifyService spotifyService,
        IYouTubeService youtubeService,
        IContentItemRepository contentRepository,
        IUnitOfWork unitOfWork,
        ILogger<CachePopulationService> logger)
    {
        _tmdbService = tmdbService;
        _spotifyService = spotifyService;
        _youtubeService = youtubeService;
        _contentRepository = contentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<ContentItem>> PopulateCacheForTypeAsync(
        ContentType contentType,
        EmotionalTag? emotion = null,
        int limit = 20)
    {
        try
        {
            _logger.LogInformation("Populating cache for type {Type} with emotion {Emotion}",
                contentType, emotion?.ToString() ?? "None");

            string query = emotion.HasValue
                ? GetSpotifyQueryForEmotion(emotion.Value)
                : "popular";

            if (contentType == ContentType.Video && emotion.HasValue)
            {
                query = GetYouTubeQueryForEmotion(emotion.Value);
            }

            var results = contentType switch
            {
                ContentType.Movie => await SearchMoviesAsync(query, emotion, limit),
                ContentType.Series => await SearchSeriesAsync(query, emotion, limit),
                ContentType.Music => await SearchMusicAsync(query, emotion, limit),
                ContentType.Video => await SearchVideosAsync(query, emotion, limit),
                _ => new List<ContentItem>()
            };

            var completeItems = FilterCompleteItems(results);

            _logger.LogInformation("Filtered {Complete} complete items from {Total} total",
                completeItems.Count, results.Count);

            await SaveContentItemsAsync(completeItems);

            return completeItems;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error populating cache for type {Type}", contentType);
            return new List<ContentItem>();
        }
    }

    private async Task<List<ContentItem>> SearchMoviesAsync(
        string query,
        EmotionalTag? emotionFilter,
        int limit)
    {
        try
        {
            List<ContentItem> results = new();

            if (emotionFilter.HasValue)
            {
                var genreIds = GetMovieGenresForEmotion(emotionFilter.Value);
                results = await _tmdbService.GetMoviesByGenresAsync(genreIds, limit * 2);
            }
            else
            {
                // Randomize criteria based on current timestamp to get varied results
                var random = new Random(DateTime.UtcNow.Millisecond);
                var criteriaIndex = random.Next(0, 3);

                results = criteriaIndex switch
                {
                    0 => await _tmdbService.GetPopularMoviesAsync(limit * 2),
                    1 => await _tmdbService.GetTopRatedMoviesAsync(limit * 2),
                    _ => await _tmdbService.GetNowPlayingMoviesAsync(limit * 2)
                };
            }

            var safeContent = FilterMentalHealthSafeContent(results);

            // Shuffle results to provide variety
            var shuffled = safeContent.OrderBy(x => Guid.NewGuid()).ToList();

            return shuffled.Take(limit).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching movies");
            return new List<ContentItem>();
        }
    }

    private async Task<List<ContentItem>> SearchSeriesAsync(
        string query,
        EmotionalTag? emotionFilter,
        int limit)
    {
        try
        {
            var results = await _tmdbService.SearchSeriesAsync(query, limit * 2);

            if (emotionFilter.HasValue)
            {
                results = results
                    .Where(s => s.EmotionalTags.Contains(emotionFilter.Value))
                    .ToList();
            }

            // Shuffle results to provide variety
            var shuffled = results.OrderBy(x => Guid.NewGuid()).Take(limit).ToList();

            return shuffled;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching series");
            return new List<ContentItem>();
        }
    }

    private async Task<List<ContentItem>> SearchMusicAsync(
        string query,
        EmotionalTag? emotionFilter,
        int limit)
    {
        try
        {
            List<ContentItem> results;

            if (emotionFilter.HasValue)
            {
                var emotionQuery = GetSpotifyQueryForEmotion(emotionFilter.Value);
                var combinedQuery = $"{query} {emotionQuery}";
                results = await _spotifyService.SearchTracksAsync(combinedQuery, limit * 2);
            }
            else
            {
                results = await _spotifyService.GetPopularTracksAsync(limit * 2);
            }

            // Shuffle results to provide variety
            var shuffled = results.OrderBy(x => Guid.NewGuid()).Take(limit).ToList();

            return shuffled;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching music");
            return new List<ContentItem>();
        }
    }

    private async Task<List<ContentItem>> SearchVideosAsync(
        string query,
        EmotionalTag? emotionFilter,
        int limit)
    {
        try
        {
            List<ContentItem> results;

            if (emotionFilter.HasValue)
            {
                var emotionQuery = GetYouTubeQueryForEmotion(emotionFilter.Value);
                var combinedQuery = $"{query} {emotionQuery}";
                results = await _youtubeService.SearchVideosAsync(combinedQuery, limit * 2);
            }
            else
            {
                results = await _youtubeService.SearchVideosAsync(query, limit * 2);
            }

            // Shuffle results to provide variety
            var shuffled = results.OrderBy(x => Guid.NewGuid()).Take(limit).ToList();

            return shuffled;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching videos");
            return new List<ContentItem>();
        }
    }

    private List<ContentItem> FilterCompleteItems(List<ContentItem> items)
    {
        return items.Where(item =>
        {
            bool hasTitle = !string.IsNullOrWhiteSpace(item.Metadata.Title);

            bool hasOverview = item.ContentType == ContentType.Music
                ? true
                : !string.IsNullOrWhiteSpace(item.Metadata.Overview);

            bool hasImage = !string.IsNullOrWhiteSpace(item.Metadata.PosterUrl) ||
                           !string.IsNullOrWhiteSpace(item.Metadata.BackdropUrl) ||
                           !string.IsNullOrWhiteSpace(item.Metadata.ThumbnailUrl) ||
                           !string.IsNullOrWhiteSpace(item.Metadata.PhotoUrl);

            bool hasEmotionalTags = item.EmotionalTags != null && item.EmotionalTags.Any();

            bool hasArtist = item.ContentType == ContentType.Music
                ? !string.IsNullOrWhiteSpace(item.Metadata.Artist)
                : true;

            return hasTitle && hasOverview && hasImage && hasEmotionalTags && hasArtist;
        }).ToList();
    }

    private async Task SaveContentItemsAsync(List<ContentItem> items)
    {
        try
        {
            foreach (var item in items)
            {
                var existingItem = await _contentRepository.FindByExternalIdAsync(item.ExternalId);

                if (existingItem == null)
                {
                    await _contentRepository.AddAsync(item);
                    _logger.LogInformation("Content item saved to database: {ExternalId}", item.ExternalId);
                }
                else
                {
                    existingItem.RefreshCache();
                    _contentRepository.Update(existingItem);
                    _logger.LogDebug("Content item already exists, updated cache: {ExternalId}", item.ExternalId);
                }
            }

            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("Saved {Count} content items to database", items.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving content items to database");
        }
    }

    private List<int> GetMovieGenresForEmotion(EmotionalTag emotion)
    {
        return EmotionToMovieGenres.ContainsKey(emotion)
            ? EmotionToMovieGenres[emotion].ToList()
            : new List<int>();
    }

    private string GetSpotifyQueryForEmotion(EmotionalTag emotion)
    {
        return EmotionToSpotifyQuery.ContainsKey(emotion)
            ? EmotionToSpotifyQuery[emotion]
            : "relaxing music";
    }

    private string GetYouTubeQueryForEmotion(EmotionalTag emotion)
    {
        return EmotionToYouTubeQuery.ContainsKey(emotion)
            ? EmotionToYouTubeQuery[emotion]
            : "meditación bienestar";
    }

    private List<ContentItem> FilterMentalHealthSafeContent(List<ContentItem> items)
    {
        return items.Where(item =>
        {
            if (item.ContentType != ContentType.Movie && item.ContentType != ContentType.Series)
            {
                return true;
            }

            var genresString = string.Join(",", item.Metadata.Genres ?? new List<string>());
            var hasRestrictedGenre = RestrictedGenresForMentalHealth.Any(restrictedId =>
                genresString.Contains(restrictedId.ToString()));

            if (hasRestrictedGenre)
            {
                _logger.LogDebug(
                    "Filtered out content '{Title}' due to restricted genre for mental health",
                    item.Metadata.Title);
                return false;
            }

            return true;
        }).ToList();
    }
}
