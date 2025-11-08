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

    // GET: api/v1/preferences - Obtener preferencias del usuario
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
            
            // Si no tiene preferencias, crear las default
            if (!preferences.Any())
            {
                var resetCommand = new ResetPreferencesCommand(userId);
                preferences = await _updateCommandService.HandleAsync(resetCommand);
            }

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

    // ✅ PUT: api/v1/preferences - Actualizar MÚLTIPLES preferencias
    [HttpPut]
    public async Task<IActionResult> UpdatePreferences([FromBody] UpdatePreferencesListRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { error = "User not authenticated" });

            if (request.Preferences == null || !request.Preferences.Any())
                return BadRequest(new { error = "No preferences provided" });

            // 🔧 Actualizar CADA preferencia en el request
            foreach (var pref in request.Preferences)
            {
                var command = new UpdatePreferencesCommand(
                    userId,
                    pref.NotificationType,
                    pref.IsEnabled,
                    pref.DeliveryMethod,
                    pref.Schedule
                );

                await _updateCommandService.HandleAsync(command);
            }

            // 🎯 Obtener TODAS las preferencias actualizadas del usuario
            var query = new GetPreferencesQuery(userId);
            var allPreferences = await _queryService.HandleAsync(query);

            var resources = allPreferences.Select(NotificationResourceAssembler.ToResource);

            return Ok(new PreferenceListResponse 
            { 
                Preferences = resources.ToList() 
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    // POST: api/v1/preferences/reset - Restablecer preferencias a default
    [HttpPost("reset")]
    public async Task<IActionResult> ResetPreferences()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { error = "User not authenticated" });

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