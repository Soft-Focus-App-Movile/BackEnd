using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftFocusBackend.Library.Application.ACL.Services;
using SoftFocusBackend.Library.Application.Internal.CommandServices;
using SoftFocusBackend.Library.Application.Internal.QueryServices;
using SoftFocusBackend.Library.Domain.Model.Commands;
using SoftFocusBackend.Library.Domain.Model.Queries;
using SoftFocusBackend.Library.Domain.Model.ValueObjects;
using SoftFocusBackend.Library.Interfaces.REST.Resources;
using SoftFocusBackend.Users.Domain.Model.ValueObjects;
using Swashbuckle.AspNetCore.Annotations;

namespace SoftFocusBackend.Library.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/library/favorites")]
[Authorize]
[Produces("application/json")]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteCommandService _favoriteCommand;
    private readonly IFavoriteQueryService _favoriteQuery;
    private readonly IUserIntegrationService _userIntegration;

    public FavoritesController(
        IFavoriteCommandService favoriteCommand,
        IFavoriteQueryService favoriteQuery,
        IUserIntegrationService userIntegration)
    {
        _favoriteCommand = favoriteCommand;
        _favoriteQuery = favoriteQuery;
        _userIntegration = userIntegration;
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "Get user favorites",
        Description = "Retrieves all favorite content items for the authenticated user. Supports filtering by content type and emotional tags. Only available for patients.",
        OperationId = "GetFavorites",
        Tags = new[] { "Favorites" }
    )]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetFavorites(
        [FromQuery] string? contentType = null,
        [FromQuery] string? emotionFilter = null)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        // Validar que sea Usuario General o Paciente
        var userType = await _userIntegration.GetUserTypeAsync(userId);
        if (userType == UserType.Psychologist)
            return Forbid("Los psicólogos no pueden tener favoritos");

        ContentType? typeFilter = null;
        if (!string.IsNullOrEmpty(contentType))
        {
            if (!Enum.TryParse<ContentType>(contentType, true, out var type))
                return BadRequest("Invalid ContentType");
            typeFilter = type;
        }

        EmotionalTag? emotionTag = null;
        if (!string.IsNullOrEmpty(emotionFilter))
        {
            if (!Enum.TryParse<EmotionalTag>(emotionFilter, true, out var emotion))
                return BadRequest("Invalid EmotionFilter");
            emotionTag = emotion;
        }

        var query = new GetFavoritesQuery(userId, typeFilter, emotionTag);
        var favorites = await _favoriteQuery.GetFavoritesAsync(query);

        var response = favorites.Select(f => new
        {
            favoriteId = f.Id,
            content = MapToContentResponse(f.Content),
            addedAt = f.AddedAt
        }).ToList();

        return Ok(new
        {
            favorites = response,
            total = response.Count
        });
    }

    [HttpPost]
    [SwaggerOperation(
        Summary = "Add content to favorites",
        Description = "Adds a content item (movie, video, audio, place) to the user's favorites list. Only available for patients.",
        OperationId = "AddFavorite",
        Tags = new[] { "Favorites" }
    )]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddFavorite([FromBody] FavoriteRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        // Validar que sea Usuario General o Paciente
        var userType = await _userIntegration.GetUserTypeAsync(userId);
        if (userType == UserType.Psychologist)
            return Forbid("Los psicólogos no pueden agregar favoritos");

        if (!Enum.TryParse<ContentType>(request.ContentType, true, out var contentType))
            return BadRequest("Invalid ContentType");

        var command = new AddFavoriteCommand(userId, request.ContentId, contentType);

        try
        {
            var favoriteId = await _favoriteCommand.AddFavoriteAsync(command);

            return Created($"/api/library/favorites/{favoriteId}", new
            {
                favoriteId,
                message = "Agregado a favoritos"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{favoriteId}")]
    [SwaggerOperation(
        Summary = "Remove content from favorites",
        Description = "Removes a content item from the user's favorites list. User must be the owner of the favorite.",
        OperationId = "RemoveFavorite",
        Tags = new[] { "Favorites" }
    )]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFavorite(string favoriteId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var command = new RemoveFavoriteCommand(userId, favoriteId);

        try
        {
            await _favoriteCommand.RemoveFavoriteAsync(command);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
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
