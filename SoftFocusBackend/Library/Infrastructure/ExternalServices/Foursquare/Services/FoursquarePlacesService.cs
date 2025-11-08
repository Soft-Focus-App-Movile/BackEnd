using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoftFocusBackend.Library.Domain.Model.Aggregates;
using SoftFocusBackend.Library.Domain.Model.ValueObjects;
using SoftFocusBackend.Library.Infrastructure.ExternalServices.Foursquare.Configuration;

namespace SoftFocusBackend.Library.Infrastructure.ExternalServices.Foursquare.Services;

/// <summary>
/// Implementación del servicio para interactuar con la API de Foursquare
/// </summary>
public class FoursquarePlacesService : IFoursquareService
{
    private readonly HttpClient _httpClient;
    private readonly FoursquareSettings _settings;
    private readonly ILogger<FoursquarePlacesService> _logger;

    // Mapeo de categorías textuales a IDs de Foursquare
    private static readonly Dictionary<string, string> CategoryMapping = new()
    {
        { "park", "16032" },
        { "cafe", "13065" },
        { "museum", "10027" },
        { "cinema", "10026" },
        { "library", "10019" },
        { "mall", "17069" },
        { "beach", "16046" },
        { "plaza", "16031" },
        { "outdoor", "16000" }
    };

    public FoursquarePlacesService(
        HttpClient httpClient,
        IOptions<FoursquareSettings> settings,
        ILogger<FoursquarePlacesService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<List<ContentItem>> SearchPlacesAsync(
        double latitude,
        double longitude,
        List<string> categoryIds,
        int radius = 5000,
        int limit = 10)
    {
        try
        {
            // Convertir nombres de categoría a IDs si es necesario
            var foursquareCategoryIds = categoryIds
                .Select(c => CategoryMapping.ContainsKey(c.ToLower()) ? CategoryMapping[c.ToLower()] : c)
                .ToList();

            var categoriesParam = string.Join(",", foursquareCategoryIds);

            // Usar InvariantCulture para asegurar que los decimales usen punto (.) en lugar de coma (,)
            var latStr = latitude.ToString("F4", CultureInfo.InvariantCulture);
            var lonStr = longitude.ToString("F4", CultureInfo.InvariantCulture);

            var url = $"{_settings.BaseUrl}/places/search?ll={latStr},{lonStr}&radius={radius}&categories={categoriesParam}&limit={limit}&sort=DISTANCE&fields=fsq_place_id,name,location,categories,distance,rating";

            _logger.LogInformation("Foursquare: Searching places with categories: {Categories}", categoriesParam);
            _logger.LogInformation("Foursquare: URL: {Url}", url);

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("X-Places-Api-Version", _settings.ApiVersion);
            _httpClient.DefaultRequestHeaders.Add("accept", "application/json");

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Foursquare API error: {StatusCode}, URL: {Url}, Response: {ErrorContent}",
                    response.StatusCode, url, errorContent);
                return new List<ContentItem>();
            }

            _logger.LogInformation("Foursquare API success: {StatusCode}", response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Foursquare API raw response: {Content}", content);

            var result = JsonSerializer.Deserialize<FoursquareSearchResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result?.Results == null)
                return new List<ContentItem>();

            var places = new List<ContentItem>();

            foreach (var place in result.Results.Take(limit))
            {
                var contentItem = await ConvertPlaceToContentItemAsync(place);
                if (contentItem != null)
                    places.Add(contentItem);
            }

            return places;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching places on Foursquare");
            return new List<ContentItem>();
        }
    }

    public async Task<ContentItem?> GetPlaceDetailsAsync(string placeId)
    {
        try
        {
            var url = $"{_settings.BaseUrl}/places/{placeId}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
            _httpClient.DefaultRequestHeaders.Add("X-Places-Api-Version", _settings.ApiVersion);
            _httpClient.DefaultRequestHeaders.Add("accept", "application/json");

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Foursquare API error: {StatusCode}", response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<FoursquarePlaceResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null)
                return null;

            return await ConvertPlaceToContentItemAsync(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting place details from Foursquare");
            return null;
        }
    }

    private async Task<ContentItem?> ConvertPlaceToContentItemAsync(FoursquarePlace place)
    {
        try
        {
            // Validar que el lugar tenga información de ubicación válida
            if (place.Location == null ||
                !place.Location.Latitude.HasValue ||
                !place.Location.Longitude.HasValue)
            {
                _logger.LogWarning("Lugar sin coordenadas válidas: {PlaceName}", place.Name);
                return null;
            }

            var externalId = ExternalContentId.CreateFoursquareId(place.FsqPlaceId ?? string.Empty);

            // No se solicitan fotos para evitar costos de API
            var metadata = ContentMetadata.CreateForPlace(
                name: place.Name ?? string.Empty,
                category: place.Categories?.FirstOrDefault()?.Name ?? "Lugar",
                address: place.Location.FormattedAddress ?? place.Location.Address ?? string.Empty,
                latitude: place.Location.Latitude.Value,
                longitude: place.Location.Longitude.Value,
                distance: place.Distance ?? 0,
                rating: place.Rating ?? 0,
                photoUrl: string.Empty
            );

            var emotionalTags = MapPlaceCategoryToEmotionalTags(
                place.Categories?.FirstOrDefault()?.Name ?? string.Empty
            );

            return ContentItem.Create(
                externalId: externalId.Value,
                contentType: ContentType.Place,
                metadata: metadata,
                emotionalTags: emotionalTags,
                externalUrl: $"https://foursquare.com/v/{place.FsqPlaceId}",
                cacheDurationHours: 6
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting Foursquare place to ContentItem");
            return null;
        }
    }

    private List<EmotionalTag> MapPlaceCategoryToEmotionalTags(string category)
    {
        var tags = new List<EmotionalTag>();
        var lowerCategory = category.ToLower();

        if (lowerCategory.Contains("park") || lowerCategory.Contains("beach") || lowerCategory.Contains("outdoor"))
            tags.Add(EmotionalTag.Calm);

        if (lowerCategory.Contains("gym") || lowerCategory.Contains("sport") || lowerCategory.Contains("recreation"))
            tags.Add(EmotionalTag.Energetic);

        if (lowerCategory.Contains("cafe") || lowerCategory.Contains("library") || lowerCategory.Contains("museum"))
            tags.Add(EmotionalTag.Calm);

        if (lowerCategory.Contains("cinema") || lowerCategory.Contains("theater") || lowerCategory.Contains("entertainment"))
            tags.Add(EmotionalTag.Happy);

        // Si no se detectó emoción, agregar Calm por defecto
        if (!tags.Any())
            tags.Add(EmotionalTag.Calm);

        return tags;
    }

    // DTOs para deserialización
    private class FoursquareSearchResponse
    {
        public List<FoursquarePlace>? Results { get; set; }
    }

    private class FoursquarePlaceResponse : FoursquarePlace
    {
    }

    private class FoursquarePlace
    {
        [System.Text.Json.Serialization.JsonPropertyName("fsq_place_id")]
        public string? FsqPlaceId { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string? Name { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("categories")]
        public List<FoursquareCategory>? Categories { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("location")]
        public FoursquareLocation? Location { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("distance")]
        public int? Distance { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("rating")]
        public double? Rating { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("photos")]
        public List<FoursquarePhoto>? Photos { get; set; }
    }

    private class FoursquareCategory
    {
        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    private class FoursquareLocation
    {
        [System.Text.Json.Serialization.JsonPropertyName("latitude")]
        public double? Latitude { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("longitude")]
        public double? Longitude { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("address")]
        public string? Address { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("formatted_address")]
        public string? FormattedAddress { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("locality")]
        public string? Locality { get; set; }
    }

    private class FoursquarePhoto
    {
        [System.Text.Json.Serialization.JsonPropertyName("prefix")]
        public string? Prefix { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("suffix")]
        public string? Suffix { get; set; }
    }
}
