using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftFocusBackend.Library.Application.Internal.QueryServices;
using SoftFocusBackend.Library.Domain.Model.Queries;
using SoftFocusBackend.Library.Domain.Model.ValueObjects;
using SoftFocusBackend.Library.Interfaces.REST.Resources;

namespace SoftFocusBackend.Library.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/library/recommendations")]
[Authorize]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationQueryService _recommendationQuery;

    public RecommendationsController(IRecommendationQueryService recommendationQuery)
    {
        _recommendationQuery = recommendationQuery;
    }

    /// <summary>
    /// Obtiene recomendaciones de lugares basadas en el clima actual
    /// </summary>
    [HttpGet("places")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetPlaceRecommendations(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] string? emotionFilter = null,
        [FromQuery] int limit = 10)
    {
        if (latitude < -90 || latitude > 90)
            return BadRequest("Latitude must be between -90 and 90");

        if (longitude < -180 || longitude > 180)
            return BadRequest("Longitude must be between -180 and 180");

        if (limit < 1 || limit > 50)
            return BadRequest("Limit must be between 1 and 50");

        EmotionalTag? emotionTag = null;
        if (!string.IsNullOrEmpty(emotionFilter))
        {
            if (!Enum.TryParse<EmotionalTag>(emotionFilter, true, out var emotion))
                return BadRequest("Invalid EmotionFilter");
            emotionTag = emotion;
        }

        var query = new GetRecommendedPlacesQuery(latitude, longitude, emotionTag, 5000, limit);

        try
        {
            var (weather, places) = await _recommendationQuery.GetRecommendedPlacesAsync(query);

            var response = new
            {
                weather = new
                {
                    temperature = weather.Temperature,
                    condition = weather.Condition,
                    description = weather.Description,
                    humidity = weather.Humidity,
                    cityName = weather.CityName
                },
                places = places.Select(MapToContentResponse).ToList(),
                totalPlaces = places.Count,
                location = new
                {
                    latitude,
                    longitude
                },
                emotionFilter = emotionTag?.ToString()
            };

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Obtiene recomendaciones de contenido basadas en una emoción
    /// </summary>
    [HttpGet("content")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetContentRecommendations(
        [FromQuery] string? contentType = null,
        [FromQuery] int limit = 10)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        if (limit < 1 || limit > 50)
            return BadRequest("Limit must be between 1 and 50");

        ContentType? typeFilter = null;
        if (!string.IsNullOrEmpty(contentType))
        {
            if (!Enum.TryParse<ContentType>(contentType, true, out var type))
                return BadRequest("Invalid ContentType");
            typeFilter = type;
        }

        var query = new GetRecommendedContentQuery(userId, typeFilter, limit);
        var recommendations = await _recommendationQuery.GetRecommendedContentAsync(query);

        var response = new
        {
            contentTypeFilter = typeFilter?.ToString(),
            content = recommendations.Select(MapToContentResponse).ToList(),
            totalRecommendations = recommendations.Count
        };

        return Ok(response);
    }

    /// <summary>
    /// Obtiene recomendaciones específicas por tipo de emoción
    /// </summary>
    [HttpGet("emotion/{emotion}")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetRecommendationsByEmotion(
        string emotion,
        [FromQuery] string? contentType = null,
        [FromQuery] int limit = 20)
    {
        if (!Enum.TryParse<EmotionalTag>(emotion, true, out var emotionalTag))
            return BadRequest("Invalid Emotion");

        if (limit < 1 || limit > 100)
            return BadRequest("limit must be between 1 and 100");

        ContentType? typeFilter = null;
        if (!string.IsNullOrEmpty(contentType))
        {
            if (!Enum.TryParse<ContentType>(contentType, true, out var type))
                return BadRequest("Invalid ContentType");
            typeFilter = type;
        }

        var query = new GetContentByEmotionQuery(emotionalTag, typeFilter, limit);
        var recommendations = await _recommendationQuery.GetContentByEmotionAsync(query);

        var response = new
        {
            emotion = emotionalTag.ToString(),
            contentTypeFilter = typeFilter?.ToString(),
            content = recommendations.Select(MapToContentResponse).ToList(),
            totalRecommendations = recommendations.Count
        };

        return Ok(response);
    }

    private static ContentItemResponse MapToContentResponse(Domain.Model.Aggregates.ContentItem item)
    {
        return new ContentItemResponse
        {
            Id = item.ExternalId,
            Type = item.ContentType.ToString(),
            Title = item.Metadata.Title,
            PosterUrl = item.Metadata.PosterUrl,
            BackdropUrl = item.Metadata.BackdropUrl,
            Rating = item.Metadata.Rating,
            Duration = item.Metadata.Duration,
            ReleaseDate = item.Metadata.ReleaseDate,
            Overview = item.Metadata.Overview,
            TrailerUrl = item.Metadata.TrailerUrl,
            Genres = item.Metadata.Genres,
            EmotionalTags = item.EmotionalTags.Select(t => t.ToString()).ToList(),
            ExternalUrl = item.ExternalUrl,
            Artist = item.Metadata.Artist,
            Album = item.Metadata.Album,
            PreviewUrl = item.Metadata.PreviewUrl,
            SpotifyUrl = item.Metadata.SpotifyUrl,
            ChannelName = item.Metadata.ChannelName,
            YouTubeUrl = item.Metadata.YouTubeUrl,
            ThumbnailUrl = item.Metadata.ThumbnailUrl,
            Category = item.Metadata.Category,
            Address = item.Metadata.Address,
            Latitude = item.Metadata.Latitude != 0 ? item.Metadata.Latitude : null,
            Longitude = item.Metadata.Longitude != 0 ? item.Metadata.Longitude : null,
            Distance = item.Metadata.Distance > 0 ? item.Metadata.Distance : null,
            PhotoUrl = item.Metadata.PhotoUrl
        };
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst("user_id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
