using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SoftFocusBackend.Library.Application.Internal.QueryServices;
using SoftFocusBackend.Library.Domain.Model.Queries;
using SoftFocusBackend.Library.Domain.Model.ValueObjects;
using SoftFocusBackend.Library.Interfaces.REST.Resources;

namespace SoftFocusBackend.Library.Interfaces.REST.Controllers;

[ApiController]
[Route("api/library")]
[Authorize]
public class ContentSearchController : ControllerBase
{
    private readonly IContentSearchQueryService _searchQuery;
    private readonly ILogger<ContentSearchController> _logger;

    public ContentSearchController(
        IContentSearchQueryService searchQuery,
        ILogger<ContentSearchController> logger)
    {
        _searchQuery = searchQuery;
        _logger = logger;
    }

    /// <summary>
    /// Busca contenido multimedia en APIs externas
    /// </summary>
    [HttpPost("search")]
    [ProducesResponseType(typeof(List<ContentItemResponse>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> SearchContent([FromBody] ContentSearchRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!Enum.TryParse<ContentType>(request.ContentType, true, out var contentType))
            return BadRequest("Invalid ContentType");

        EmotionalTag? emotionFilter = null;
        if (!string.IsNullOrEmpty(request.EmotionFilter))
        {
            if (!Enum.TryParse<EmotionalTag>(request.EmotionFilter, true, out var emotion))
                return BadRequest("Invalid EmotionFilter");
            emotionFilter = emotion;
        }

        var query = new SearchContentQuery(
            request.Query,
            contentType,
            emotionFilter,
            request.Limit
        );

        _logger.LogInformation("Controller: Calling SearchContentAsync with query: {Query}, type: {Type}",
            request.Query, contentType);

        var results = await _searchQuery.SearchContentAsync(query);

        _logger.LogInformation("Controller: Received {Count} results from service", results.Count);

        var response = results.Select(MapToResponse).ToList();

        _logger.LogInformation("Controller: Mapped {Count} items to response", response.Count);

        return Ok(new
        {
            results = response,
            totalResults = results.Count,
            page = 1
        });
    }

    private static ContentItemResponse MapToResponse(Domain.Model.Aggregates.ContentItem item)
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
            Overview = item.Metadata.Overview,
            TrailerUrl = item.Metadata.TrailerUrl,
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
