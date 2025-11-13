using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftFocusBackend.Users.Application.Internal.CommandServices;
using SoftFocusBackend.Users.Application.Internal.QueryServices;
using SoftFocusBackend.Users.Domain.Model.Commands;
using SoftFocusBackend.Users.Domain.Model.Queries;
using SoftFocusBackend.Users.Domain.Model.ValueObjects;
using SoftFocusBackend.Users.Interfaces.REST.Resources;
using SoftFocusBackend.Users.Interfaces.REST.Transform;
using System.Security.Claims;
using SoftFocusBackend.Users.Domain.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace SoftFocusBackend.Users.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/users")]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private readonly IUserCommandService _userCommandService;
    private readonly IUserQueryService _userQueryService;
    private readonly IPsychologistQueryService _psychologistQueryService;
    private readonly ILogger<UserController> _logger;

    public UserController(
        IUserCommandService userCommandService,
        IUserQueryService userQueryService,
        IPsychologistQueryService psychologistQueryService,
        ILogger<UserController> logger)
    {
        _userCommandService = userCommandService ?? throw new ArgumentNullException(nameof(userCommandService));
        _userQueryService = userQueryService ?? throw new ArgumentNullException(nameof(userQueryService));
        _psychologistQueryService = psychologistQueryService ?? throw new ArgumentNullException(nameof(psychologistQueryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("profile")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Get user profile",
        Description = "Retrieves the complete profile information of the authenticated user, including personal details and settings.",
        OperationId = "GetProfile",
        Tags = new[] { "User Profile" }
    )]
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
    [SwaggerOperation(
        Summary = "Update user profile",
        Description = "Updates the profile information of the authenticated user. Supports profile image upload. Use multipart/form-data.",
        OperationId = "UpdateProfile",
        Tags = new[] { "User Profile" }
    )]
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

            // Get current user to preserve existing data
            var query = new GetUserByIdQuery(userId, true, userId);
            var currentUser = await _userQueryService.HandleGetUserByIdAsync(query);

            if (currentUser == null)
            {
                _logger.LogWarning("User not found for profile update: {UserId}", userId);
                return NotFound(UserResourceAssembler.ToErrorResponse("User not found"));
            }

            byte[]? imageBytes = null;
            string? imageFileName = null;

            if (resource.ProfileImage != null && resource.ProfileImage.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await resource.ProfileImage.CopyToAsync(memoryStream);
                imageBytes = memoryStream.ToArray();
                imageFileName = resource.ProfileImage.FileName;
            }

            var command = UserResourceAssembler.ToUpdateCommandWithImage(resource, userId, currentUser.FullName, imageBytes, imageFileName);
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
    [SwaggerOperation(
        Summary = "Delete user account",
        Description = "Deletes or deactivates the authenticated user's account. Use hardDelete=true for permanent deletion (not recommended).",
        OperationId = "DeleteProfile",
        Tags = new[] { "User Profile" }
    )]
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

    [HttpGet("psychologists/directory")]
    [SwaggerOperation(
        Summary = "Get psychologists directory",
        Description = "Retrieves a paginated and filterable list of verified psychologists. Supports filtering by specialties, city, rating, and more. Public endpoint.",
        OperationId = "GetPsychologistsDirectory",
        Tags = new[] { "Psychologists" }
    )]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPsychologistsDirectory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] List<PsychologySpecialty>? specialties = null,
        [FromQuery] string? city = null,
        [FromQuery] double? minRating = null,
        [FromQuery] bool? isAcceptingNewPatients = null,
        [FromQuery] List<string>? languages = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        try
        {
            _logger.LogInformation("Public psychologists directory request - Page: {Page}, Size: {PageSize}",
                page, pageSize);

            var query = new GetPsychologistsDirectoryQuery(
                page: page,
                pageSize: pageSize,
                specialties: specialties,
                city: city,
                minRating: minRating,
                isAcceptingNewPatients: isAcceptingNewPatients,
                languages: languages,
                searchTerm: searchTerm,
                sortBy: sortBy,
                sortDescending: sortDescending
            );

            if (!query.IsValid())
            {
                return BadRequest(UserResourceAssembler.ToErrorResponse("Invalid query parameters"));
            }

            var (psychologists, totalCount) = await _psychologistQueryService.HandleGetPsychologistsDirectoryAsync(query);

            var response = new
            {
                psychologists = psychologists.Select(PsychologistResourceAssembler.ToDirectoryResource),
                pagination = new
                {
                    page,
                    pageSize,
                    totalCount,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    hasNextPage = page * pageSize < totalCount,
                    hasPreviousPage = page > 1
                },
                filters = new
                {
                    specialties,
                    city,
                    minRating,
                    isAcceptingNewPatients,
                    languages,
                    searchTerm,
                    sortBy,
                    sortDescending
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting psychologists directory");
            return StatusCode(StatusCodes.Status500InternalServerError,
                UserResourceAssembler.ToErrorResponse("An error occurred while retrieving psychologists directory"));
        }
    }

    [HttpGet("psychologists/{id}")]
    [SwaggerOperation(
        Summary = "Get psychologist details",
        Description = "Retrieves detailed information about a specific psychologist by ID. Only shows verified and active psychologist profiles. Public endpoint.",
        OperationId = "GetPsychologistById",
        Tags = new[] { "Psychologists" }
    )]
    [ProducesResponseType(typeof(PsychologistDirectoryResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPsychologistById(string id)
    {
        try
        {
            _logger.LogInformation("Public psychologist detail request: {PsychologistId}", id);

            var psychologist = await _psychologistQueryService.HandleGetPsychologistByIdAsync(id);

            if (psychologist == null)
            {
                _logger.LogWarning("Psychologist not found: {PsychologistId}", id);
                return NotFound(UserResourceAssembler.ToErrorResponse("Psychologist not found"));
            }

            if (!psychologist.IsVerified || !psychologist.IsActive || !psychologist.IsProfileVisibleInDirectory)
            {
                _logger.LogWarning("Psychologist profile not accessible: {PsychologistId}", id);
                return NotFound(UserResourceAssembler.ToErrorResponse("Psychologist profile not available"));
            }

            var response = PsychologistResourceAssembler.ToDirectoryResource(psychologist);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting psychologist details: {PsychologistId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                UserResourceAssembler.ToErrorResponse("An error occurred while retrieving psychologist details"));
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