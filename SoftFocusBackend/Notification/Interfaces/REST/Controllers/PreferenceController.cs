using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SoftFocusBackend.Notification.Application.Internal.CommandServices;
using SoftFocusBackend.Notification.Application.Internal.QueryServices;
using SoftFocusBackend.Notification.Domain.Model.Commands;
using SoftFocusBackend.Notification.Domain.Model.Queries;
using SoftFocusBackend.Notification.Interfaces.REST.Resources;
using SoftFocusBackend.Notification.Interfaces.REST.Transform;
using Swashbuckle.AspNetCore.Annotations;

namespace SoftFocusBackend.Notification.Interfaces.REST.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/preferences")]
[Produces("application/json")]
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
    [SwaggerOperation(
        Summary = "Get notification preferences",
        Description = "Retrieves all notification preferences for the authenticated user. Creates default preferences if none exist.",
        OperationId = "GetPreferences",
        Tags = new[] { "Notification Preferences" }
    )]
    [ProducesResponseType(typeof(PreferenceListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

    [HttpPut]
    [SwaggerOperation(
        Summary = "Update notification preferences",
        Description = "Updates multiple notification preferences at once for the authenticated user.",
        OperationId = "UpdatePreferences",
        Tags = new[] { "Notification Preferences" }
    )]
    [ProducesResponseType(typeof(PreferenceListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdatePreferences([FromBody] UpdatePreferencesListRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { error = "User not authenticated" });

            if (request.Preferences == null || !request.Preferences.Any())
                return BadRequest(new { error = "No preferences provided" });

            // ✅ PASO 1: Obtener preferencias actuales ANTES de actualizar
            var currentQuery = new GetPreferencesQuery(userId);
            var currentPreferences = await _queryService.HandleAsync(currentQuery);
            
            // Crear diccionario para acceso rápido al estado anterior
            var currentDict = currentPreferences.ToDictionary(
                p => p.NotificationType,
                p => p.IsEnabled
            );

            // ✅ PASO 2: Actualizar cada preferencia CON estado anterior
            foreach (var pref in request.Preferences)
            {
                // Obtener el estado anterior de esta preferencia
                bool? previousEnabled = currentDict.ContainsKey(pref.NotificationType)
                    ? currentDict[pref.NotificationType]
                    : null;

                var command = new UpdatePreferencesCommand(
                    userId,
                    pref.NotificationType,
                    pref.IsEnabled,
                    pref.DeliveryMethod,
                    pref.Schedule,
                    previousEnabled // ✅ Pasar el estado anterior para detectar cambios
                );

                await _updateCommandService.HandleAsync(command);
            }

            // ✅ PASO 3: Obtener todas las preferencias actualizadas
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

    [HttpPost("reset")]
    [SwaggerOperation(
        Summary = "Reset notification preferences",
        Description = "Resets all notification preferences to their default values for the authenticated user.",
        OperationId = "ResetPreferences",
        Tags = new[] { "Notification Preferences" }
    )]
    [ProducesResponseType(typeof(PreferenceListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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