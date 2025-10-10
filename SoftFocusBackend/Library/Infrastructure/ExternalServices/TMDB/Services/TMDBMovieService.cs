using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Model.ValueObjects;
using SoftFocusBackend.Library.Infrastructure.ExternalServices.TMDB.Configuration;

namespace SoftFocusBackend.Library.Infrastructure.ExternalServices.TMDB.Services;

/// <summary>
/// Implementación del servicio para interactuar con la API de TMDB
/// </summary>
public class TMDBMovieService : ITMDBService
{
    private readonly HttpClient _httpClient;
    private readonly TMDBSettings _settings;
    private readonly ILogger<TMDBMovieService> _logger;

    public TMDBMovieService(
        HttpClient httpClient,
        IOptions<TMDBSettings> settings,
        ILogger<TMDBMovieService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<List<ContentItem>> SearchMoviesAsync(string query, int limit = 20)
    {
        try
        {
            var url = $"{_settings.BaseUrl}/search/movie?api_key={_settings.ApiKey}&query={Uri.EscapeDataString(query)}&language={_settings.Language}";

            _logger.LogInformation("TMDB: Searching movies with query: {Query}", query);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("TMDB API error: {StatusCode}", response.StatusCode);
                return new List<ContentItem>();
            }

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("TMDB: Received response, length: {Length} chars", content.Length);

            var result = JsonSerializer.Deserialize<TMDBSearchResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result?.Results == null)
            {
                _logger.LogWarning("TMDB: Deserialization returned null results");
                return new List<ContentItem>();
            }

            _logger.LogInformation("TMDB: Deserialized {Count} movies from TMDB", result.Results.Count);

            var movies = new List<ContentItem>();
            var count = 0;
            var failedCount = 0;

            foreach (var movie in result.Results)
            {
                if (count >= limit) break;

                try
                {
                    _logger.LogInformation("TMDB: Converting movie ID {MovieId}: {Title}", movie.Id, movie.Title);
                    var contentItem = await ConvertMovieToContentItemAsync(movie);
                    if (contentItem != null)
                    {
                        movies.Add(contentItem);
                        count++;
                        _logger.LogInformation("TMDB: Successfully converted movie {MovieId}", movie.Id);
                    }
                    else
                    {
                        failedCount++;
                        _logger.LogWarning("TMDB: Conversion returned null for movie {MovieId}", movie.Id);
                    }
                }
                catch (Exception ex)
                {
                    failedCount++;
                    _logger.LogError(ex, "TMDB: Failed to convert movie {MovieId}: {Title}", movie.Id, movie.Title);
                }
            }

            _logger.LogInformation("TMDB: Conversion complete. Success: {Success}, Failed: {Failed}", count, failedCount);
            return movies;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching movies on TMDB");
            return new List<ContentItem>();
        }
    }

    public async Task<List<ContentItem>> SearchSeriesAsync(string query, int limit = 20)
    {
        try
        {
            var url = $"{_settings.BaseUrl}/search/tv?api_key={_settings.ApiKey}&query={Uri.EscapeDataString(query)}&language={_settings.Language}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("TMDB API error: {StatusCode}", response.StatusCode);
                return new List<ContentItem>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TMDBSearchResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result?.Results == null)
                return new List<ContentItem>();

            var series = new List<ContentItem>();
            var count = 0;

            foreach (var show in result.Results)
            {
                if (count >= limit) break;

                var contentItem = await ConvertSeriesToContentItemAsync(show);
                if (contentItem != null)
                {
                    series.Add(contentItem);
                    count++;
                }
            }

            return series;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching series on TMDB");
            return new List<ContentItem>();
        }
    }

    public async Task<List<ContentItem>> SearchMoviesByGenresAsync(string query, List<int> genreIds, int limit = 20)
    {
        try
        {
            var url = $"{_settings.BaseUrl}/search/movie?api_key={_settings.ApiKey}&query={Uri.EscapeDataString(query)}&language={_settings.Language}";

            _logger.LogInformation("TMDB: Searching movies by genres with query: {Query}, genreIds: {GenreIds}",
                query, string.Join(",", genreIds));

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("TMDB API error: {StatusCode}", response.StatusCode);
                return new List<ContentItem>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TMDBSearchResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result?.Results == null)
            {
                _logger.LogWarning("TMDB: Deserialization returned null results");
                return new List<ContentItem>();
            }

            _logger.LogInformation("TMDB: Received {Count} movies from API", result.Results.Count);

            // Log sample movie genres for debugging
            if (result.Results.Any())
            {
                var firstMovie = result.Results.First();
                _logger.LogInformation("TMDB: Sample movie '{Title}' has GenreIds: {GenreIds}",
                    firstMovie.Title,
                    firstMovie.GenreIds != null ? string.Join(",", firstMovie.GenreIds) : "null");
            }

            // Filtrar por géneros ANTES de convertir
            var filteredMovies = result.Results;
            if (genreIds != null && genreIds.Any())
            {
                _logger.LogInformation("TMDB: Filtering by requested genreIds: {RequestedGenres}", string.Join(",", genreIds));

                filteredMovies = result.Results
                    .Where(m => m.GenreIds != null && m.GenreIds.Any(gId => genreIds.Contains(gId)))
                    .ToList();

                _logger.LogInformation("TMDB: After genre filter, {Count} movies match", filteredMovies.Count);
            }

            var movies = new List<ContentItem>();
            var count = 0;

            foreach (var movie in filteredMovies)
            {
                if (count >= limit) break;

                try
                {
                    var contentItem = await ConvertMovieToContentItemAsync(movie);
                    if (contentItem != null)
                    {
                        movies.Add(contentItem);
                        count++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "TMDB: Failed to convert movie {MovieId}: {Title}", movie.Id, movie.Title);
                }
            }

            _logger.LogInformation("TMDB: Returning {Count} movies after genre filtering", movies.Count);
            return movies;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching movies by genres on TMDB");
            return new List<ContentItem>();
        }
    }

    public async Task<ContentItem?> GetMovieDetailsAsync(int movieId)
    {
        try
        {
            var url = $"{_settings.BaseUrl}/movie/{movieId}?api_key={_settings.ApiKey}&language={_settings.Language}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("TMDB API error: {StatusCode}", response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var movie = JsonSerializer.Deserialize<TMDBMovie>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (movie == null)
                return null;

            return await ConvertMovieToContentItemAsync(movie);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting movie details from TMDB");
            return null;
        }
    }

    public async Task<ContentItem?> GetSeriesDetailsAsync(int seriesId)
    {
        try
        {
            var url = $"{_settings.BaseUrl}/tv/{seriesId}?api_key={_settings.ApiKey}&language={_settings.Language}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("TMDB API error: {StatusCode}", response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var series = JsonSerializer.Deserialize<TMDBMovie>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (series == null)
                return null;

            return await ConvertSeriesToContentItemAsync(series);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting series details from TMDB");
            return null;
        }
    }

    private async Task<ContentItem?> ConvertMovieToContentItemAsync(TMDBMovie movie)
    {
        try
        {
            _logger.LogInformation("TMDB: Creating ExternalContentId for movie {MovieId}", movie.Id);
            var externalId = ExternalContentId.CreateTmdbId(movie.Id.ToString(), ContentType.Movie);

            _logger.LogInformation("TMDB: Getting trailer for movie {MovieId}", movie.Id);
            var trailerUrl = await GetTrailerUrlAsync(movie.Id, "movie");

            _logger.LogInformation("TMDB: Creating metadata for movie {MovieId} - Title: {Title}, PosterPath: {Poster}",
                movie.Id, movie.Title, movie.PosterPath);

            var metadata = ContentMetadata.CreateForMovie(
                title: movie.Title ?? string.Empty,
                overview: movie.Overview ?? string.Empty,
                posterUrl: _settings.GetPosterUrl(movie.PosterPath ?? string.Empty),
                backdropUrl: _settings.GetBackdropUrl(movie.BackdropPath ?? string.Empty),
                rating: movie.VoteAverage,
                duration: movie.Runtime > 0 ? $"{movie.Runtime}min" : string.Empty,
                trailerUrl: trailerUrl,
                genres: movie.Genres?.Select(g => g.Name).ToList() ?? new List<string>()
            );

            _logger.LogInformation("TMDB: Mapping genres to emotional tags for movie {MovieId}", movie.Id);
            var emotionalTags = MapGenresToEmotionalTags(movie.GenreIds ?? new List<int>());

            _logger.LogInformation("TMDB: Creating ContentItem for movie {MovieId} with externalId: {ExternalId}",
                movie.Id, externalId.Value);

            var contentItem = ContentItem.Create(
                externalId: externalId.Value,
                contentType: ContentType.Movie,
                metadata: metadata,
                emotionalTags: emotionalTags,
                externalUrl: $"https://www.themoviedb.org/movie/{movie.Id}",
                cacheDurationHours: 24
            );

            _logger.LogInformation("TMDB: ContentItem created successfully for movie {MovieId}", movie.Id);
            return contentItem;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting TMDB movie {MovieId} to ContentItem. Title: {Title}, GenreIds: {GenreIds}",
                movie.Id, movie.Title, string.Join(",", movie.GenreIds ?? new List<int>()));
            return null;
        }
    }

    private async Task<ContentItem?> ConvertSeriesToContentItemAsync(TMDBMovie series)
    {
        try
        {
            var externalId = ExternalContentId.CreateTmdbId(series.Id.ToString(), ContentType.Series);
            var trailerUrl = await GetTrailerUrlAsync(series.Id, "tv");

            var metadata = ContentMetadata.CreateForMovie(
                title: series.Name ?? series.Title ?? string.Empty,
                overview: series.Overview ?? string.Empty,
                posterUrl: _settings.GetPosterUrl(series.PosterPath ?? string.Empty),
                backdropUrl: _settings.GetBackdropUrl(series.BackdropPath ?? string.Empty),
                rating: series.VoteAverage,
                duration: series.NumberOfSeasons > 0 ? $"{series.NumberOfSeasons} temporadas" : string.Empty,
                trailerUrl: trailerUrl,
                genres: series.Genres?.Select(g => g.Name).ToList() ?? new List<string>()
            );

            var emotionalTags = MapGenresToEmotionalTags(series.GenreIds ?? new List<int>());

            return ContentItem.Create(
                externalId: externalId.Value,
                contentType: ContentType.Series,
                metadata: metadata,
                emotionalTags: emotionalTags,
                externalUrl: $"https://www.themoviedb.org/tv/{series.Id}",
                cacheDurationHours: 24
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting TMDB series to ContentItem");
            return null;
        }
    }

    private async Task<string> GetTrailerUrlAsync(int id, string type)
    {
        try
        {
            var url = $"{_settings.BaseUrl}/{type}/{id}/videos?api_key={_settings.ApiKey}&language={_settings.Language}";

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return string.Empty;

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TMDBVideosResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var trailer = result?.Results?
                .FirstOrDefault(v => v.Type == "Trailer" && v.Site == "YouTube");

            return trailer != null ? _settings.GetTrailerUrl(trailer.Key) : string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private List<EmotionalTag> MapGenresToEmotionalTags(List<int> genreIds)
    {
        var tags = new List<EmotionalTag>();

        // Mapeo de géneros TMDB a emociones
        if (genreIds.Contains(35) || genreIds.Contains(10751) || genreIds.Contains(10749))
            tags.Add(EmotionalTag.Happy); // Comedy, Family, Romance

        if (genreIds.Contains(28) || genreIds.Contains(12) || genreIds.Contains(878))
            tags.Add(EmotionalTag.Energetic); // Action, Adventure, Sci-Fi

        if (genreIds.Contains(18) || genreIds.Contains(99) || genreIds.Contains(36))
            tags.Add(EmotionalTag.Calm); // Drama, Documentary, History

        if (genreIds.Contains(18) || genreIds.Contains(10749))
            tags.Add(EmotionalTag.Sad); // Drama, Romance

        return tags.Distinct().ToList();
    }

    // DTOs para deserialización
    private class TMDBSearchResponse
    {
        public List<TMDBMovie>? Results { get; set; }
    }

    private class TMDBMovie
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Name { get; set; }
        public string? Overview { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("poster_path")]
        public string? PosterPath { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("backdrop_path")]
        public string? BackdropPath { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("vote_average")]
        public double VoteAverage { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("genre_ids")]
        public List<int>? GenreIds { get; set; }

        public List<TMDBGenre>? Genres { get; set; }
        public int Runtime { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("number_of_seasons")]
        public int NumberOfSeasons { get; set; }
    }

    private class TMDBGenre
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private class TMDBVideosResponse
    {
        public List<TMDBVideo>? Results { get; set; }
    }

    private class TMDBVideo
    {
        public string Key { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Site { get; set; } = string.Empty;
    }
}
