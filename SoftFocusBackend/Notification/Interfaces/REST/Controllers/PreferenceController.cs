using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
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
    private readonly ILogger<PreferenceController> _logger;

    public PreferenceController(
        UpdatePreferencesCommandService updateCommandService,
        PreferenceQueryService queryService,
        ILogger<PreferenceController> logger)
    {
        _updateCommandService = updateCommandService;
        _queryService = queryService;
        _logger = logger;
    }

    // GET: api/v1/preferences
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
            _logger.LogError(ex, "Error getting preferences");
            return BadRequest(new { error = ex.Message });
        }
    }

    // PUT: api/v1/preferences
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

            // 🔍 DEBUG: Log del request completo
            _logger.LogInformation("=== UPDATE PREFERENCES REQUEST ===");
            _logger.LogInformation($"UserId: {userId}");
            _logger.LogInformation($"Preferences count: {request.Preferences.Count}");

            foreach (var pref in request.Preferences)
            {
                _logger.LogInformation($"--- Processing preference: {pref.NotificationType} ---");
                _logger.LogInformation($"IsEnabled: {pref.IsEnabled}");
                _logger.LogInformation($"DeliveryMethod: {pref.DeliveryMethod}");
                _logger.LogInformation($"Schedule is null: {pref.Schedule == null}");
                
                if (pref.Schedule != null)
                {
                    _logger.LogInformation($"Schedule StartTime: {pref.Schedule.StartTime}");
                    _logger.LogInformation($"Schedule EndTime: {pref.Schedule.EndTime}");
                    _logger.LogInformation($"Schedule DaysOfWeek: {string.Join(",", pref.Schedule.DaysOfWeek)}");
                }

                var command = new UpdatePreferencesCommand(
                    userId,
                    pref.NotificationType,
                    pref.IsEnabled,
                    pref.DeliveryMethod,
                    pref.Schedule
                );

                var updatedPref = await _updateCommandService.HandleAsync(command);
                
                // 🔍 DEBUG: Log de lo que se guardó
                _logger.LogInformation($"Updated preference Schedule is null: {updatedPref.Schedule == null}");
                if (updatedPref.Schedule != null)
                {
                    _logger.LogInformation($"Saved Schedule StartTime: {updatedPref.Schedule.QuietHours.FirstOrDefault()?.StartTime}");
                }
            }

            // Obtener TODAS las preferencias actualizadas
            var query = new GetPreferencesQuery(userId);
            var allPreferences = await _queryService.HandleAsync(query);

            // 🔍 DEBUG: Log de lo que se retorna
            _logger.LogInformation("=== RETURNING PREFERENCES ===");
            foreach (var pref in allPreferences)
            {
                _logger.LogInformation($"{pref.NotificationType}: Schedule is null = {pref.Schedule == null}");
            }

            var resources = allPreferences.Select(NotificationResourceAssembler.ToResource);

            return Ok(new PreferenceListResponse 
            { 
                Preferences = resources.ToList() 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating preferences");
            return BadRequest(new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    // POST: api/v1/preferences/reset
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
            _logger.LogError(ex, "Error resetting preferences");
            return BadRequest(new { error = ex.Message });
        }
    }
}