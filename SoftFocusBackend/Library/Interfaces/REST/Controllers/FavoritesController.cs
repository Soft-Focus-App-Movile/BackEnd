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

namespace SoftFocusBackend.Library.Interfaces.REST.Controllers;

[ApiController]
[Route("api/library/favorites")]
[Authorize]
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

    /// <summary>
    /// Obtiene todos los favoritos del usuario autenticado
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
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

    /// <summary>
    /// Agrega un contenido a favoritos
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
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

    /// <summary>
    /// Elimina un contenido de favoritos
    /// </summary>
    [HttpDelete("{favoriteId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
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
