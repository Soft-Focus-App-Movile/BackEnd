using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Model.ValueObjects;
using SoftFocusBackend.Library.Infrastructure.ExternalServices.YouTube.Configuration;

namespace SoftFocusBackend.Library.Infrastructure.ExternalServices.YouTube.Services;

/// <summary>
/// Implementación del servicio para interactuar con la API de YouTube
/// </summary>
public class YouTubeVideoService : IYouTubeService
{
    private readonly HttpClient _httpClient;
    private readonly YouTubeSettings _settings;
    private readonly ILogger<YouTubeVideoService> _logger;

    public YouTubeVideoService(
        HttpClient httpClient,
        IOptions<YouTubeSettings> settings,
        ILogger<YouTubeVideoService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<List<ContentItem>> SearchVideosAsync(string query, int limit = 20)
    {
        try
        {
            var url = $"{_settings.BaseUrl}/search?part=snippet&q={Uri.EscapeDataString(query)}&type=video&maxResults={limit}&key={_settings.ApiKey}&relevanceLanguage=es&videoDuration={_settings.VideoDuration}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("YouTube API error: {StatusCode}", response.StatusCode);
                return new List<ContentItem>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<YouTubeSearchResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result?.Items == null)
                return new List<ContentItem>();

            var videos = new List<ContentItem>();

            foreach (var item in result.Items.Take(limit))
            {
                var contentItem = ConvertVideoToContentItem(item);
                if (contentItem != null)
                    videos.Add(contentItem);
            }

            return videos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching videos on YouTube");
            return new List<ContentItem>();
        }
    }

    public async Task<List<ContentItem>> SearchVideosByEmotionQueryAsync(string emotionQuery, int limit = 20)
    {
        return await SearchVideosAsync(emotionQuery, limit);
    }

    public async Task<ContentItem?> GetVideoDetailsAsync(string videoId)
    {
        try
        {
            var url = $"{_settings.BaseUrl}/videos?part=snippet&id={videoId}&key={_settings.ApiKey}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("YouTube API error: {StatusCode}", response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<YouTubeSearchResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var video = result?.Items?.FirstOrDefault();
            if (video == null)
                return null;

            return ConvertVideoToContentItem(video);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting video details from YouTube");
            return null;
        }
    }

    private ContentItem? ConvertVideoToContentItem(YouTubeItem item)
    {
        try
        {
            var videoId = item.Id?.VideoId ?? item.Id?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(videoId))
                return null;

            var externalId = ExternalContentId.CreateYouTubeId(videoId);

            var metadata = ContentMetadata.CreateForVideo(
                title: item.Snippet?.Title ?? string.Empty,
                overview: item.Snippet?.Description ?? string.Empty,
                thumbnailUrl: item.Snippet?.Thumbnails?.High?.Url ?? string.Empty,
                channelName: item.Snippet?.ChannelTitle ?? string.Empty,
                youtubeUrl: _settings.GetVideoUrl(videoId)
            );

            var emotionalTags = MapVideoToEmotionalTags(
                item.Snippet?.Title ?? string.Empty,
                item.Snippet?.Description ?? string.Empty
            );

            return ContentItem.Create(
                externalId: externalId.Value,
                contentType: ContentType.Video,
                metadata: metadata,
                emotionalTags: emotionalTags,
                externalUrl: _settings.GetVideoUrl(videoId),
                cacheDurationHours: 24
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting YouTube video to ContentItem");
            return null;
        }
    }

    private List<EmotionalTag> MapVideoToEmotionalTags(string title, string description)
    {
        var tags = new List<EmotionalTag>();
        var combined = $"{title} {description}".ToLower();

        // Mapeo por palabras clave
        if (combined.Contains("ansiedad") || combined.Contains("anxiety") || combined.Contains("estrés"))
            tags.Add(EmotionalTag.Anxious);

        if (combined.Contains("meditación") || combined.Contains("meditation") || combined.Contains("relajación") ||
            combined.Contains("calm") || combined.Contains("mindfulness"))
            tags.Add(EmotionalTag.Calm);

        if (combined.Contains("energía") || combined.Contains("energy") || combined.Contains("yoga") ||
            combined.Contains("ejercicio"))
            tags.Add(EmotionalTag.Energetic);

        if (combined.Contains("tristeza") || combined.Contains("sad") || combined.Contains("sanación"))
            tags.Add(EmotionalTag.Sad);

        if (combined.Contains("feliz") || combined.Contains("happy") || combined.Contains("alegría"))
            tags.Add(EmotionalTag.Happy);

        // Si no se detectó emoción, agregar Calm (videos de bienestar tienden a ser calmantes)
        if (!tags.Any())
            tags.Add(EmotionalTag.Calm);

        return tags;
    }

    // DTOs para deserialización
    private class YouTubeSearchResponse
    {
        public List<YouTubeItem>? Items { get; set; }
    }

    private class YouTubeItem
    {
        public YouTubeId? Id { get; set; }
        public YouTubeSnippet? Snippet { get; set; }
    }

    private class YouTubeId
    {
        public string? VideoId { get; set; }
    }

    private class YouTubeSnippet
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ChannelTitle { get; set; }
        public YouTubeThumbnails? Thumbnails { get; set; }
    }

    private class YouTubeThumbnails
    {
        public YouTubeThumbnail? High { get; set; }
    }

    private class YouTubeThumbnail
    {
        public string Url { get; set; } = string.Empty;
    }
}
