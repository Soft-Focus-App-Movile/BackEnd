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
[Route("api/v1/preferences")]
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

    // GET: api/v1/preferences - Obtener preferencias
    [HttpGet]
    public async Task<IActionResult> GetPreferences()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { error = "User not authenticated" });

            var query = new GetPreferencesQuery(userId);
            var preferences = await _queryService.HandleAsync(query);
            var resources = preferences.Select(NotificationResourceAssembler.ToResource);

            return Ok(new PreferenceListResponse 
            { 
                Preferences = resources.ToList() 
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // PUT: api/v1/preferences - Actualizar preferencias
    [HttpPut]
    public async Task<IActionResult> UpdatePreferences([FromBody] UpdatePreferenceRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { error = "User not authenticated" });

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

    // POST: api/v1/preferences/reset - Restablecer preferencias
    [HttpPost("reset")]
    public async Task<IActionResult> ResetPreferences()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { error = "User not authenticated" });

            // 🆕 USAR EL NUEVO COMMAND PARA RESET
            var resetCommand = new ResetPreferencesCommand(userId);
            var resetPreferences = await _updateCommandService.HandleAsync(resetCommand);
            
            var resources = resetPreferences.Select(NotificationResourceAssembler.ToResource);

            return Ok(new PreferenceListResponse 
            { 
                Preferences = resources.ToList(),
                Message = "Preferences reset to default values successfully"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}