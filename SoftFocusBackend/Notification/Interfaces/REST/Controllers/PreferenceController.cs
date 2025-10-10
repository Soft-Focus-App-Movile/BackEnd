using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SoftFocusBackend.Notification.Application.Internal.CommandServices;
using SoftFocusBackend.Notification.Application.Internal.QueryServices;
using SoftFocusBackend.Notification.Domain.Model.Commands;
using SoftFocusBackend.Notification.Domain.Model.Queries;
using SoftFocusBackend.Notification.Interfaces.REST.Resources;
using SoftFocusBackend.Notification.Interfaces.REST.Transform;

namespace SoftFocusBackend.Notification.Interfaces.REST.Controllers;

[Authorize]
[ApiController]
[Route("api/preferences")]
public class PreferenceController : ControllerBase
{
    private readonly UpdatePreferencesCommandService _updateCommandService;
    private readonly PreferenceQueryService _queryService;

    public PreferenceController(
        UpdatePreferencesCommandService updateCommandService,
        PreferenceQueryService queryService)
    {
        _updateCommandService = updateCommandService;
        _queryService = queryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPreferences()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var query = new GetPreferencesQuery(userId);
        var preferences = await _queryService.HandleAsync(query);
        var resources = preferences.Select(NotificationResourceAssembler.ToResource);

        return Ok(resources);
    }

    [HttpPut]
    public async Task<IActionResult> UpdatePreferences([FromBody] UpdatePreferenceRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        try
        {
            var command = new UpdatePreferencesCommand(
                userId,
                request.NotificationType,
                request.IsEnabled,
                request.DeliveryMethod,
                request.Schedule
            );

            var preference = await _updateCommandService.HandleAsync(command);
            var resource = NotificationResourceAssembler.ToResource(preference);

            return Ok(resource);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}