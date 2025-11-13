using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SoftFocusBackend.Library.Application.Internal.QueryServices;
using SoftFocusBackend.Library.Domain.Model.Queries;
using SoftFocusBackend.Library.Domain.Model.ValueObjects;
using SoftFocusBackend.Library.Interfaces.REST.Resources;
using Swashbuckle.AspNetCore.Annotations;

namespace SoftFocusBackend.Library.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/library")]
[Authorize]
[Produces("application/json")]
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

    [HttpGet("{contentId}")]
    [SwaggerOperation(
        Summary = "Get content by ID",
        Description = "Retrieves a specific library content item (article, video, exercise, audio) by its unique identifier.",
        OperationId = "GetContentById",
        Tags = new[] { "Library" }
    )]
    [ProducesResponseType(typeof(ContentItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetContentById(string contentId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var query = new GetContentByIdQuery(contentId);

        try
        {
            var content = await _searchQuery.GetContentByIdAsync(query);

            if (content == null)
                return NotFound(new { message = $"Content with ID {contentId} not found" });

            var response = MapToResponse(content);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching content by ID: {ContentId}", contentId);
            return StatusCode(500, new { message = "Error retrieving content" });
        }
    }

    [HttpPost("search")]
    [SwaggerOperation(
        Summary = "Search library content",
        Description = "Searches the library for content items (articles, videos, exercises, audios) based on keywords, content type, and emotional tags.",
        OperationId = "SearchContent",
        Tags = new[] { "Library" }
    )]
    [ProducesResponseType(typeof(List<ContentItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
