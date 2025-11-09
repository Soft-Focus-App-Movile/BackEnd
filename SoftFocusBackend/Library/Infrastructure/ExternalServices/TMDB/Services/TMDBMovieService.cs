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

    public async Task<List<ContentItem>> GetPopularMoviesAsync(int limit = 20)
    {
        try
        {
            // Randomize page to get varied results (TMDB has pages 1-500)
            var random = new Random();
            var page = random.Next(1, 6); // Pages 1-5 to ensure good quality content

            var url = $"{_settings.BaseUrl}/movie/popular?api_key={_settings.ApiKey}&language={_settings.Language}&page={page}";

            _logger.LogInformation("TMDB: Getting popular movies from page {Page}", page);

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

            _logger.LogInformation("TMDB: Received {Count} popular movies", result.Results.Count);

            var movies = new List<ContentItem>();
            var count = 0;

            foreach (var movie in result.Results)
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

            _logger.LogInformation("TMDB: Returning {Count} popular movies", movies.Count);
            return movies;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting popular movies from TMDB");
            return new List<ContentItem>();
        }
    }

    public async Task<List<ContentItem>> GetTopRatedMoviesAsync(int limit = 20)
    {
        try
        {
            // Randomize page to get varied results
            var random = new Random();
            var page = random.Next(1, 6); // Pages 1-5

            var url = $"{_settings.BaseUrl}/movie/top_rated?api_key={_settings.ApiKey}&language={_settings.Language}&page={page}";

            _logger.LogInformation("TMDB: Getting top rated movies from page {Page}", page);

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

            _logger.LogInformation("TMDB: Received {Count} top rated movies", result.Results.Count);

            var movies = new List<ContentItem>();
            var count = 0;

            foreach (var movie in result.Results)
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

            _logger.LogInformation("TMDB: Returning {Count} top rated movies", movies.Count);
            return movies;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top rated movies from TMDB");
            return new List<ContentItem>();
        }
    }

    public async Task<List<ContentItem>> GetNowPlayingMoviesAsync(int limit = 20)
    {
        try
        {
            // Randomize page to get varied results
            var random = new Random();
            var page = random.Next(1, 4); // Pages 1-3 for now playing

            var url = $"{_settings.BaseUrl}/movie/now_playing?api_key={_settings.ApiKey}&language={_settings.Language}&page={page}";

            _logger.LogInformation("TMDB: Getting now playing movies from page {Page}", page);

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

            _logger.LogInformation("TMDB: Received {Count} now playing movies", result.Results.Count);

            var movies = new List<ContentItem>();
            var count = 0;

            foreach (var movie in result.Results)
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

            _logger.LogInformation("TMDB: Returning {Count} now playing movies", movies.Count);
            return movies;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting now playing movies from TMDB");
            return new List<ContentItem>();
        }
    }

    public async Task<List<ContentItem>> GetMoviesByGenresAsync(List<int> genreIds, int limit = 20)
    {
        try
        {
            // Randomize page to get varied results
            var random = new Random();
            var page = random.Next(1, 6); // Pages 1-5

            var genreParam = string.Join(",", genreIds);
            var url = $"{_settings.BaseUrl}/discover/movie?api_key={_settings.ApiKey}&language={_settings.Language}&with_genres={genreParam}&sort_by=popularity.desc&page={page}";

            _logger.LogInformation("TMDB: Discovering movies with genres: {Genres} from page {Page}", genreParam, page);

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

            _logger.LogInformation("TMDB: Received {Count} movies for genres {Genres}", result.Results.Count, genreParam);

            var movies = new List<ContentItem>();
            var count = 0;

            foreach (var movie in result.Results)
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

            _logger.LogInformation("TMDB: Returning {Count} movies for genres {Genres}", movies.Count, genreParam);
            return movies;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error discovering movies by genres from TMDB");
            return new List<ContentItem>();
        }
    }

    private async Task<ContentItem?> ConvertMovieToContentItemAsync(TMDBMovie movie)
    {
        try
        {
            // Validar que tenga los datos esenciales
            if (string.IsNullOrWhiteSpace(movie.Title))
            {
                _logger.LogWarning("TMDB: Skipping movie {MovieId} - missing title", movie.Id);
                return null;
            }

            if (string.IsNullOrWhiteSpace(movie.PosterPath))
            {
                _logger.LogWarning("TMDB: Skipping movie {MovieId} ({Title}) - missing poster", movie.Id, movie.Title);
                return null;
            }

            if (string.IsNullOrWhiteSpace(movie.Overview))
            {
                _logger.LogWarning("TMDB: Skipping movie {MovieId} ({Title}) - missing overview", movie.Id, movie.Title);
                return null;
            }

            if (movie.VoteAverage <= 0)
            {
                _logger.LogWarning("TMDB: Skipping movie {MovieId} ({Title}) - missing rating", movie.Id, movie.Title);
                return null;
            }

            _logger.LogInformation("TMDB: Creating ExternalContentId for movie {MovieId}", movie.Id);
            var externalId = ExternalContentId.CreateTmdbId(movie.Id.ToString(), ContentType.Movie);

            // Si no tenemos runtime (viene de /search), obtener detalles completos con trailer en una sola llamada
            int runtime = movie.Runtime;
            string releaseDate = movie.ReleaseDate ?? string.Empty;
            var genres = movie.Genres;
            string trailerUrl = string.Empty;

            if (movie.Runtime == 0)
            {
                _logger.LogInformation("TMDB: Movie {MovieId} missing runtime, fetching full details with trailer", movie.Id);
                var fullDetails = await GetMovieFullDetailsAsync(movie.Id);

                if (fullDetails != null)
                {
                    runtime = fullDetails.Runtime;
                    releaseDate = fullDetails.ReleaseDate ?? string.Empty;
                    genres = fullDetails.Genres;
                    trailerUrl = ExtractTrailerUrl(fullDetails);
                }
                else
                {
                    _logger.LogWarning("TMDB: Could not fetch full details for movie {MovieId}, using partial data", movie.Id);
                }
            }
            else
            {
                // Ya tenemos runtime, solo obtener trailer
                _logger.LogInformation("TMDB: Getting trailer for movie {MovieId}", movie.Id);
                trailerUrl = await GetTrailerUrlAsync(movie.Id, "movie");
            }

            // Trailer es opcional, no descartamos la película si no lo tiene
            if (string.IsNullOrWhiteSpace(trailerUrl))
            {
                _logger.LogDebug("TMDB: Movie {MovieId} ({Title}) has no trailer, continuing anyway", movie.Id, movie.Title);
            }

            // Validar que tenga runtime
            if (runtime == 0)
            {
                _logger.LogWarning("TMDB: Skipping movie {MovieId} ({Title}) - missing runtime", movie.Id, movie.Title);
                return null;
            }

            // Validar que tenga releaseDate
            if (string.IsNullOrWhiteSpace(releaseDate))
            {
                _logger.LogWarning("TMDB: Skipping movie {MovieId} ({Title}) - missing release date", movie.Id, movie.Title);
                return null;
            }

            _logger.LogInformation("TMDB: Creating metadata for movie {MovieId} - Title: {Title}, Runtime: {Runtime}",
                movie.Id, movie.Title, runtime);

            var metadata = ContentMetadata.CreateForMovie(
                title: movie.Title,
                overview: movie.Overview,
                posterUrl: _settings.GetPosterUrl(movie.PosterPath),
                backdropUrl: _settings.GetBackdropUrl(movie.BackdropPath ?? string.Empty),
                rating: movie.VoteAverage,
                duration: runtime > 0 ? $"{runtime}min" : string.Empty,
                releaseDate: releaseDate,
                trailerUrl: trailerUrl,
                genres: genres?.Select(g => g.Name).ToList() ?? new List<string>()
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
            // Validar que tenga los datos esenciales
            var title = series.Name ?? series.Title;
            if (string.IsNullOrWhiteSpace(title))
            {
                _logger.LogWarning("TMDB: Skipping series {SeriesId} - missing title", series.Id);
                return null;
            }

            if (string.IsNullOrWhiteSpace(series.PosterPath))
            {
                _logger.LogWarning("TMDB: Skipping series {SeriesId} ({Title}) - missing poster", series.Id, title);
                return null;
            }

            if (string.IsNullOrWhiteSpace(series.Overview))
            {
                _logger.LogWarning("TMDB: Skipping series {SeriesId} ({Title}) - missing overview", series.Id, title);
                return null;
            }

            if (series.VoteAverage <= 0)
            {
                _logger.LogWarning("TMDB: Skipping series {SeriesId} ({Title}) - missing rating", series.Id, title);
                return null;
            }

            var externalId = ExternalContentId.CreateTmdbId(series.Id.ToString(), ContentType.Series);

            // Si no tenemos número de temporadas (viene de /search), obtener detalles completos con trailer en una sola llamada
            int numberOfSeasons = series.NumberOfSeasons;
            string firstAirDate = series.FirstAirDate ?? string.Empty;
            var genres = series.Genres;
            string trailerUrl = string.Empty;

            if (series.NumberOfSeasons == 0)
            {
                _logger.LogInformation("TMDB: Series {SeriesId} missing seasons, fetching full details with trailer", series.Id);
                var fullDetails = await GetSeriesFullDetailsAsync(series.Id);

                if (fullDetails != null)
                {
                    numberOfSeasons = fullDetails.NumberOfSeasons;
                    firstAirDate = fullDetails.FirstAirDate ?? string.Empty;
                    genres = fullDetails.Genres;
                    trailerUrl = ExtractTrailerUrl(fullDetails);
                }
                else
                {
                    _logger.LogWarning("TMDB: Could not fetch full details for series {SeriesId}, using partial data", series.Id);
                }
            }
            else
            {
                // Ya tenemos temporadas, solo obtener trailer
                _logger.LogInformation("TMDB: Getting trailer for series {SeriesId}", series.Id);
                trailerUrl = await GetTrailerUrlAsync(series.Id, "tv");
            }

            // Trailer es opcional, no descartamos la serie si no lo tiene
            if (string.IsNullOrWhiteSpace(trailerUrl))
            {
                _logger.LogDebug("TMDB: Series {SeriesId} ({Title}) has no trailer, continuing anyway", series.Id, title);
            }

            // Validar que tenga número de temporadas
            if (numberOfSeasons == 0)
            {
                _logger.LogWarning("TMDB: Skipping series {SeriesId} ({Title}) - missing seasons", series.Id, title);
                return null;
            }

            // Validar que tenga firstAirDate
            if (string.IsNullOrWhiteSpace(firstAirDate))
            {
                _logger.LogWarning("TMDB: Skipping series {SeriesId} ({Title}) - missing first air date", series.Id, title);
                return null;
            }

            _logger.LogInformation("TMDB: Creating metadata for series {SeriesId} - Title: {Title}, Seasons: {Seasons}",
                series.Id, title, numberOfSeasons);

            var metadata = ContentMetadata.CreateForMovie(
                title: title,
                overview: series.Overview,
                posterUrl: _settings.GetPosterUrl(series.PosterPath),
                backdropUrl: _settings.GetBackdropUrl(series.BackdropPath ?? string.Empty),
                rating: series.VoteAverage,
                duration: numberOfSeasons > 0 ? $"{numberOfSeasons} temporadas" : string.Empty,
                releaseDate: firstAirDate,
                trailerUrl: trailerUrl,
                genres: genres?.Select(g => g.Name).ToList() ?? new List<string>()
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

    /// <summary>
    /// Obtiene detalles completos de una película incluyendo runtime y videos en una sola llamada
    /// </summary>
    private async Task<TMDBMovieWithVideos?> GetMovieFullDetailsAsync(int movieId)
    {
        try
        {
            var url = $"{_settings.BaseUrl}/movie/{movieId}?api_key={_settings.ApiKey}&language={_settings.Language}&append_to_response=videos";

            _logger.LogInformation("TMDB: Getting full movie details with videos for {MovieId}", movieId);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("TMDB API error getting movie details: {StatusCode}", response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var movie = JsonSerializer.Deserialize<TMDBMovieWithVideos>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return movie;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting full movie details from TMDB for {MovieId}", movieId);
            return null;
        }
    }

    /// <summary>
    /// Obtiene detalles completos de una serie incluyendo seasons y videos en una sola llamada
    /// </summary>
    private async Task<TMDBMovieWithVideos?> GetSeriesFullDetailsAsync(int seriesId)
    {
        try
        {
            var url = $"{_settings.BaseUrl}/tv/{seriesId}?api_key={_settings.ApiKey}&language={_settings.Language}&append_to_response=videos";

            _logger.LogInformation("TMDB: Getting full series details with videos for {SeriesId}", seriesId);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("TMDB API error getting series details: {StatusCode}", response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var series = JsonSerializer.Deserialize<TMDBMovieWithVideos>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return series;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting full series details from TMDB for {SeriesId}", seriesId);
            return null;
        }
    }

    /// <summary>
    /// Extrae la URL del trailer de un objeto TMDBMovieWithVideos
    /// </summary>
    private string ExtractTrailerUrl(TMDBMovieWithVideos? movieWithVideos)
    {
        if (movieWithVideos?.Videos?.Results == null)
        {
            _logger.LogDebug("TMDB: No videos object in response");
            return string.Empty;
        }

        if (!movieWithVideos.Videos.Results.Any())
        {
            _logger.LogDebug("TMDB: Videos Results is empty");
            return string.Empty;
        }

        _logger.LogDebug("TMDB: Found {Count} videos, types: {Types}",
            movieWithVideos.Videos.Results.Count,
            string.Join(", ", movieWithVideos.Videos.Results.Select(v => $"{v.Type}@{v.Site}")));

        var trailer = movieWithVideos.Videos.Results
            .FirstOrDefault(v => v.Type == "Trailer" && v.Site == "YouTube");

        if (trailer == null)
        {
            _logger.LogDebug("TMDB: No YouTube trailer found in videos");
            return string.Empty;
        }

        return _settings.GetTrailerUrl(trailer.Key);
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

        [System.Text.Json.Serialization.JsonPropertyName("release_date")]
        public string? ReleaseDate { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("first_air_date")]
        public string? FirstAirDate { get; set; }
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

    /// <summary>
    /// DTO para película/serie con videos incluidos (append_to_response=videos)
    /// </summary>
    private class TMDBMovieWithVideos
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

        [System.Text.Json.Serialization.JsonPropertyName("release_date")]
        public string? ReleaseDate { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("first_air_date")]
        public string? FirstAirDate { get; set; }

        public TMDBVideosResponse? Videos { get; set; }
    }
}
