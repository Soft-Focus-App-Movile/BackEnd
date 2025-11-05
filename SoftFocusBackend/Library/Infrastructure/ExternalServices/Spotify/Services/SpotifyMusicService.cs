using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Model.ValueObjects;
using SoftFocusBackend.Library.Infrastructure.ExternalServices.Spotify.Configuration;

namespace SoftFocusBackend.Library.Infrastructure.ExternalServices.Spotify.Services;

/// <summary>
/// Implementación del servicio para interactuar con la API de Spotify
/// </summary>
public class SpotifyMusicService : ISpotifyService
{
    private readonly HttpClient _httpClient;
    private readonly SpotifySettings _settings;
    private readonly ILogger<SpotifyMusicService> _logger;

    // Token management (mutable state separate from immutable settings)
    private string? _accessToken;
    private DateTime? _tokenExpiresAt;

    public SpotifyMusicService(
        HttpClient httpClient,
        IOptions<SpotifySettings> settings,
        ILogger<SpotifyMusicService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<List<ContentItem>> SearchTracksAsync(string query, int limit = 20)
    {
        try
        {
            await EnsureValidTokenAsync();

            var url = $"{_settings.BaseUrl}/search?q={Uri.EscapeDataString(query)}&type=track&market=PE&limit={limit}";

            _logger.LogInformation("Spotify: Using access token: {Token}",
                string.IsNullOrEmpty(_accessToken) ? "NULL" : $"{_accessToken[..10]}...");

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Spotify API error: {StatusCode}, Response: {ErrorContent}",
                    response.StatusCode, errorContent);
                return new List<ContentItem>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<SpotifySearchResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result?.Tracks?.Items == null)
                return new List<ContentItem>();

            var tracks = new List<ContentItem>();

            foreach (var track in result.Tracks.Items.Take(limit))
            {
                var contentItem = ConvertTrackToContentItem(track);
                if (contentItem != null)
                    tracks.Add(contentItem);
            }

            return tracks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching tracks on Spotify");
            return new List<ContentItem>();
        }
    }

    public async Task<List<ContentItem>> SearchTracksByEmotionQueryAsync(string emotionQuery, int limit = 20)
    {
        return await SearchTracksAsync(emotionQuery, limit);
    }

    public async Task<ContentItem?> GetTrackDetailsAsync(string trackId)
    {
        try
        {
            await EnsureValidTokenAsync();

            var url = $"{_settings.BaseUrl}/tracks/{trackId}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Spotify API error: {StatusCode}", response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var track = JsonSerializer.Deserialize<SpotifyTrack>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (track == null)
                return null;

            return ConvertTrackToContentItem(track);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting track details from Spotify");
            return null;
        }
    }

    public async Task<List<ContentItem>> GetPopularTracksAsync(int limit = 20)
    {
        try
        {
            await EnsureValidTokenAsync();

            var queries = new[] { "top hits 2024", "trending music", "viral songs", "popular latin" };
            var allTracks = new List<ContentItem>();

            foreach (var query in queries)
            {
                if (allTracks.Count >= limit) break;

                var url = $"{_settings.BaseUrl}/search?q={Uri.EscapeDataString(query)}&type=track&market=PE&limit={Math.Min(10, limit - allTracks.Count)}";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Spotify API error for query '{Query}': {StatusCode}", query, response.StatusCode);
                    continue;
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<SpotifySearchResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result?.Tracks?.Items != null)
                {
                    foreach (var track in result.Tracks.Items)
                    {
                        if (allTracks.Count >= limit) break;

                        var contentItem = ConvertTrackToContentItem(track);
                        if (contentItem != null && !allTracks.Any(t => t.ExternalId == contentItem.ExternalId))
                        {
                            allTracks.Add(contentItem);
                        }
                    }
                }
            }

            _logger.LogInformation("Spotify: Returning {Count} popular tracks", allTracks.Count);
            return allTracks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting popular tracks from Spotify");
            return new List<ContentItem>();
        }
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        try
        {
            var authValue = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.ClientSecret}"));

            var request = new HttpRequestMessage(HttpMethod.Post, _settings.AuthUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authValue);
            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Spotify authentication failed: {StatusCode}", response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Spotify: Token response content: {Content}", content);

            var result = JsonSerializer.Deserialize<SpotifyTokenResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _logger.LogInformation("Spotify: Deserialized result - IsNull: {IsNull}, AccessToken: {Token}",
                result == null, result?.AccessToken ?? "NULL");

            if (result != null && !string.IsNullOrEmpty(result.AccessToken))
            {
                _accessToken = result.AccessToken;
                _tokenExpiresAt = DateTime.UtcNow.AddSeconds(result.ExpiresIn - 60);

                _logger.LogInformation("Spotify: Token obtained successfully, expires at: {ExpiresAt}", _tokenExpiresAt);
                return result.AccessToken;
            }

            _logger.LogWarning("Spotify: Token deserialization failed or token was empty");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Spotify access token");
            return null;
        }
    }

    private async Task EnsureValidTokenAsync()
    {
        if (!IsTokenValid())
        {
            await GetAccessTokenAsync();
        }
    }

    private bool IsTokenValid()
    {
        return !string.IsNullOrEmpty(_accessToken)
               && _tokenExpiresAt.HasValue
               && _tokenExpiresAt.Value > DateTime.UtcNow.AddMinutes(5);
    }

    private ContentItem? ConvertTrackToContentItem(SpotifyTrack track)
    {
        try
        {
            var externalId = ExternalContentId.CreateSpotifyId(track.Id);

            var artistNames = string.Join(", ", track.Artists?.Select(a => a.Name) ?? new List<string>());
            var albumImage = track.Album?.Images?.FirstOrDefault()?.Url ?? string.Empty;
            var durationMs = track.DurationMs;
            var durationMinutes = durationMs > 0 ? $"{durationMs / 60000}:{(durationMs % 60000) / 1000:D2}" : string.Empty;

            var metadata = ContentMetadata.CreateForMusic(
                title: track.Name ?? string.Empty,
                artist: artistNames,
                album: track.Album?.Name ?? string.Empty,
                posterUrl: albumImage,
                duration: durationMinutes,
                previewUrl: track.PreviewUrl ?? string.Empty,
                spotifyUrl: track.ExternalUrls?.Spotify ?? string.Empty
            );

            var emotionalTags = MapTrackToEmotionalTags(track.Name, artistNames);

            return ContentItem.Create(
                externalId: externalId.Value,
                contentType: ContentType.Music,
                metadata: metadata,
                emotionalTags: emotionalTags,
                externalUrl: track.ExternalUrls?.Spotify ?? string.Empty,
                cacheDurationHours: 24
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting Spotify track to ContentItem");
            return null;
        }
    }

    private List<EmotionalTag> MapTrackToEmotionalTags(string trackName, string artistName)
    {
        var tags = new List<EmotionalTag>();
        var combined = $"{trackName} {artistName}".ToLower();

        // Mapeo básico por palabras clave
        if (combined.Contains("happy") || combined.Contains("alegre") || combined.Contains("feliz"))
            tags.Add(EmotionalTag.Happy);

        if (combined.Contains("calm") || combined.Contains("relax") || combined.Contains("peace"))
            tags.Add(EmotionalTag.Calm);

        if (combined.Contains("energy") || combined.Contains("workout") || combined.Contains("motivat"))
            tags.Add(EmotionalTag.Energetic);

        if (combined.Contains("sad") || combined.Contains("melanchol") || combined.Contains("triste"))
            tags.Add(EmotionalTag.Sad);

        // Si no se detectó ninguna emoción, agregar Calm por defecto
        if (!tags.Any())
            tags.Add(EmotionalTag.Calm);

        return tags;
    }

    // DTOs para deserialización
    private class SpotifySearchResponse
    {
        public SpotifyTracksContainer? Tracks { get; set; }
    }

    private class SpotifyTracksContainer
    {
        public List<SpotifyTrack>? Items { get; set; }
    }

    private class SpotifyTrack
    {
        public string Id { get; set; } = string.Empty;
        public string? Name { get; set; }
        public List<SpotifyArtist>? Artists { get; set; }
        public SpotifyAlbum? Album { get; set; }
        public int DurationMs { get; set; }
        public string? PreviewUrl { get; set; }
        public SpotifyExternalUrls? ExternalUrls { get; set; }
    }

    private class SpotifyArtist
    {
        public string Name { get; set; } = string.Empty;
    }

    private class SpotifyAlbum
    {
        public string? Name { get; set; }
        public List<SpotifyImage>? Images { get; set; }
    }

    private class SpotifyImage
    {
        public string Url { get; set; } = string.Empty;
    }

    private class SpotifyExternalUrls
    {
        public string? Spotify { get; set; }
    }

    private class SpotifyTokenResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
