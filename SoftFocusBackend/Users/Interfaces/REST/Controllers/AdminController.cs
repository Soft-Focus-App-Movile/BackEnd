using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftFocusBackend.Users.Application.Internal.CommandServices;
using SoftFocusBackend.Users.Application.Internal.QueryServices;
using SoftFocusBackend.Users.Domain.Model.Commands;
using SoftFocusBackend.Users.Domain.Model.Queries;
using SoftFocusBackend.Users.Domain.Model.ValueObjects;
using SoftFocusBackend.Users.Interfaces.REST.Transform;
using System.Security.Claims;
using SoftFocusBackend.Users.Domain.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace SoftFocusBackend.Users.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/users")]
[Produces("application/json")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IUserQueryService _userQueryService;
    private readonly IPsychologistCommandService _psychologistCommandService;
    private readonly IUserCommandService _userCommandService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IUserQueryService userQueryService,
        IPsychologistCommandService psychologistCommandService,
        IUserCommandService userCommandService,
        ILogger<AdminController> logger)
    {
        _userQueryService = userQueryService ?? throw new ArgumentNullException(nameof(userQueryService));
        _psychologistCommandService = psychologistCommandService ?? throw new ArgumentNullException(nameof(psychologistCommandService));
        _userCommandService = userCommandService ?? throw new ArgumentNullException(nameof(userCommandService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all users (Admin)",
        Description = "Retrieves a paginated and filterable list of all users in the system. Only accessible by admins.",
        OperationId = "GetAllUsers",
        Tags = new[] { "Admin" }
    )]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] UserType? userType = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] bool? isVerified = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        try
        {
            var adminId = GetCurrentUserId();
            _logger.LogInformation("Admin {AdminId} requesting users list - Page: {Page}, Size: {PageSize}", 
                adminId, page, pageSize);

            var query = new GetAllUsersQuery(page, pageSize, userType, isActive, isVerified, 
                searchTerm, sortBy, sortDescending, adminId);

            var (users, totalCount) = await _userQueryService.HandleGetAllUsersAsync(query);

            var response = new
            {
                users = users.Select(UserResourceAssembler.ToAdminUserResource),
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
                    userType,
                    isActive,
                    isVerified,
                    searchTerm,
                    sortBy,
                    sortDescending
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users list for admin");
            return StatusCode(StatusCodes.Status500InternalServerError,
                UserResourceAssembler.ToErrorResponse("An error occurred while retrieving users"));
        }
    }

    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Get user details (Admin)",
        Description = "Retrieves detailed information about a specific user. Only accessible by admins.",
        OperationId = "GetUserAdmin",
        Tags = new[] { "Admin" }
    )]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUser(string id)
    {
        try
        {
            var adminId = GetCurrentUserId();
            _logger.LogInformation("Admin {AdminId} requesting user details: {UserId}", adminId, id);

            var query = new GetUserByIdQuery(id, true, adminId);
            var user = await _userQueryService.HandleGetUserByIdAsync(query);

            if (user == null)
            {
                _logger.LogWarning("User not found: {UserId}", id);
                return NotFound(UserResourceAssembler.ToErrorResponse("User not found"));
            }

            var response = UserResourceAssembler.ToAdminUserDetailResource(user);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user details for admin: {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                UserResourceAssembler.ToErrorResponse("An error occurred while retrieving user details"));
        }
    }

    [HttpPut("{id}/verify")]
    [SwaggerOperation(
        Summary = "Verify psychologist (Admin)",
        Description = "Approves or rejects a psychologist's registration after reviewing their credentials. Only accessible by admins.",
        OperationId = "VerifyPsychologist",
        Tags = new[] { "Admin" }
    )]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> VerifyPsychologist(string id, [FromBody] VerifyPsychologistRequest request)
    {
        try
        {
            var adminId = GetCurrentUserId();
            var adminName = GetCurrentUserName();

            _logger.LogInformation("Admin {AdminId} verifying psychologist: {PsychologistId} - Approved: {IsApproved}", 
                adminId, id, request.IsApproved);

            if (!ModelState.IsValid)
            {
                return BadRequest(UserResourceAssembler.ToErrorResponse("Invalid request data"));
            }

            var command = new VerifyPsychologistCommand(id, adminName ?? adminId ?? "Unknown Admin", 
                request.IsApproved, request.Notes);

            var success = await _psychologistCommandService.HandleVerifyPsychologistAsync(command);

            if (!success)
            {
                return BadRequest(UserResourceAssembler.ToErrorResponse("Failed to verify psychologist"));
            }

            var response = new
            {
                success = true,
                message = request.IsApproved ? "Psychologist verified successfully" : "Psychologist verification rejected",
                verifiedBy = adminName ?? adminId,
                verifiedAt = DateTime.UtcNow,
                notes = request.Notes
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying psychologist: {PsychologistId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                UserResourceAssembler.ToErrorResponse("An error occurred while verifying psychologist"));
        }
    }

    [HttpPut("{id}/status")]
    [SwaggerOperation(
        Summary = "Change user status (Admin)",
        Description = "Activates or deactivates a user account. Only accessible by admins.",
        OperationId = "ChangeUserStatus",
        Tags = new[] { "Admin" }
    )]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ChangeUserStatus(string id, [FromBody] ChangeUserStatusRequest request)
    {
        try
        {
            var adminId = GetCurrentUserId();
            _logger.LogInformation("Admin {AdminId} changing user status: {UserId} - Active: {IsActive}", 
                adminId, id, request.IsActive);

            if (!ModelState.IsValid)
            {
                return BadRequest(UserResourceAssembler.ToErrorResponse("Invalid request data"));
            }

            var user = await _userQueryService.HandleGetUserByIdAsync(new GetUserByIdQuery(id, false, adminId));
            if (user == null)
            {
                return NotFound(UserResourceAssembler.ToErrorResponse("User not found"));
            }

            if (request.IsActive)
            {
                user.Activate();
            }
            else
            {
                user.Deactivate();
            }

            var command = new UpdateUserProfileCommand(id, user.FullName);
            var updatedUser = await _userCommandService.HandleUpdateUserProfileAsync(command);

            if (updatedUser == null)
            {
                return BadRequest(UserResourceAssembler.ToErrorResponse("Failed to change user status"));
            }

            var response = new
            {
                success = true,
                message = request.IsActive ? "User activated successfully" : "User deactivated successfully",
                userId = id,
                isActive = request.IsActive,
                updatedBy = adminId,
                updatedAt = DateTime.UtcNow,
                reason = request.Reason
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing user status: {UserId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                UserResourceAssembler.ToErrorResponse("An error occurred while changing user status"));
        }
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst("user_id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    private string? GetCurrentUserName()
    {
        return User.FindFirst("full_name")?.Value ?? User.FindFirst(ClaimTypes.Name)?.Value;
    }
}

public record VerifyPsychologistRequest
{
    public bool IsApproved { get; init; }
    public string? Notes { get; init; }
}

public record ChangeUserStatusRequest
{
    public bool IsActive { get; init; }
    public string? Reason { get; init; }
}