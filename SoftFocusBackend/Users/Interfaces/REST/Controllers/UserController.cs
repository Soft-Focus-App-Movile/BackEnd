using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftFocusBackend.Users.Application.Internal.CommandServices;
using SoftFocusBackend.Users.Application.Internal.QueryServices;
using SoftFocusBackend.Users.Domain.Model.Commands;
using SoftFocusBackend.Users.Domain.Model.Queries;
using SoftFocusBackend.Users.Interfaces.REST.Resources;
using SoftFocusBackend.Users.Interfaces.REST.Transform;
using System.Security.Claims;
using SoftFocusBackend.Users.Domain.Services;

namespace SoftFocusBackend.Users.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/users")]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private readonly IUserCommandService _userCommandService;
    private readonly IUserQueryService _userQueryService;
    private readonly ILogger<UserController> _logger;

    public UserController(
        IUserCommandService userCommandService,
        IUserQueryService userQueryService,
        ILogger<UserController> logger)
    {
        _userCommandService = userCommandService ?? throw new ArgumentNullException(nameof(userCommandService));
        _userQueryService = userQueryService ?? throw new ArgumentNullException(nameof(userQueryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(UserProfileResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("Get profile attempt without valid user ID");
                return Unauthorized(UserResourceAssembler.ToErrorResponse("Invalid user session"));
            }

            _logger.LogInformation("Get profile request for user: {UserId}", userId);

            var query = new GetUserByIdQuery(userId, true, userId);
            var user = await _userQueryService.HandleGetUserByIdAsync(query);

            if (user == null)
            {
                _logger.LogWarning("User profile not found: {UserId}", userId);
                return NotFound(UserResourceAssembler.ToErrorResponse("User profile not found"));
            }

            var response = UserResourceAssembler.ToProfileResource(user);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                UserResourceAssembler.ToErrorResponse("An error occurred while retrieving profile"));
        }
    }

    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType(typeof(UserProfileResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProfile([FromForm] UpdateUserProfileResource resource)  // CAMBIAR A FromForm
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("Update profile attempt without valid user ID");
                return Unauthorized(UserResourceAssembler.ToErrorResponse("Invalid user session"));
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid update profile request for user: {UserId}", userId);
                return BadRequest(UserResourceAssembler.ToErrorResponse("Invalid request data"));
            }

            _logger.LogInformation("Update profile request for user: {UserId}", userId);
            
            byte[]? imageBytes = null;
            string? imageFileName = null;
            
            if (resource.ProfileImage != null && resource.ProfileImage.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await resource.ProfileImage.CopyToAsync(memoryStream);
                imageBytes = memoryStream.ToArray();
                imageFileName = resource.ProfileImage.FileName;
            }

            var command = UserResourceAssembler.ToUpdateCommandWithImage(resource, userId, imageBytes, imageFileName);  // CAMBIAR ESTE MÉTODO
            var updatedUser = await _userCommandService.HandleUpdateUserProfileAsync(command);

            if (updatedUser == null)
            {
                _logger.LogWarning("Failed to update profile for user: {UserId}", userId);
                return BadRequest(UserResourceAssembler.ToErrorResponse("Failed to update profile"));
            }

            var response = UserResourceAssembler.ToProfileResource(updatedUser);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile");
            return StatusCode(StatusCodes.Status500InternalServerError,
                UserResourceAssembler.ToErrorResponse("An error occurred while updating profile"));
        }
    }

    [HttpDelete("profile")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteProfile([FromQuery] bool hardDelete = false)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("Delete profile attempt without valid user ID");
                return Unauthorized(UserResourceAssembler.ToErrorResponse("Invalid user session"));
            }

            _logger.LogInformation("Delete profile request for user: {UserId} (Hard: {HardDelete})", userId, hardDelete);

            var command = new DeleteUserCommand(userId, "User requested account deletion", hardDelete, userId);
            var success = await _userCommandService.HandleDeleteUserAsync(command);

            if (!success)
            {
                _logger.LogWarning("Failed to delete profile for user: {UserId}", userId);
                return BadRequest(UserResourceAssembler.ToErrorResponse("Failed to delete account"));
            }

            return Ok(new 
            { 
                success = true, 
                message = hardDelete ? "Account permanently deleted" : "Account deactivated successfully",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user profile");
            return StatusCode(StatusCodes.Status500InternalServerError,
                UserResourceAssembler.ToErrorResponse("An error occurred while deleting account"));
        }
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst("user_id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    private string? GetClientIpAddress()
    {
        try
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }
        catch
        {
            return null;
        }
    }

    private string? GetClientUserAgent()
    {
        try
        {
            return HttpContext.Request.Headers.UserAgent.FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }
}