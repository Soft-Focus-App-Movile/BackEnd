using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftFocusBackend.Notification.Application.Internal.CommandServices;
using SoftFocusBackend.Notification.Application.Internal.QueryServices;
using SoftFocusBackend.Notification.Domain.Model.Commands;
using SoftFocusBackend.Notification.Domain.Model.Queries;
using SoftFocusBackend.Notification.Interfaces.REST.Resources;
using SoftFocusBackend.Notification.Interfaces.REST.Transform;

// Alias para evitar conflicto de nombres
using NotificationAggregate = SoftFocusBackend.Notification.Domain.Model.Aggregates.Notification;

namespace SoftFocusBackend.Notification.Interfaces.REST.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/notifications")]
public class NotificationController : ControllerBase
{
    private readonly SendNotificationCommandService _sendCommandService;
    private readonly NotificationHistoryQueryService _historyQueryService;
    private readonly PreferenceQueryService _preferenceQueryService;

    public NotificationController(
        SendNotificationCommandService sendCommandService,
        NotificationHistoryQueryService historyQueryService,
        PreferenceQueryService preferenceQueryService)
    {
        _sendCommandService = sendCommandService;
        _historyQueryService = historyQueryService;
        _preferenceQueryService = preferenceQueryService;
    }

    [HttpPost]
    public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequest request)
    {
        try
        {
            var command = new SendNotificationCommand(
                request.UserId,
                request.Type,
                request.Title,
                request.Content,
                request.Priority,
                request.DeliveryMethod,
                request.ScheduledAt,
                request.Metadata
            );

            var notification = await _sendCommandService.HandleAsync(command);
            if (notification == null)
                return BadRequest(new { error = "Notification could not be created." });

            // Uso del alias NotificationAggregate
            var resource = NotificationResourceAssembler.ToResource(notification as NotificationAggregate);

            return Ok(resource);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetNotificationHistory(
        string userId,
        [FromQuery] string? type = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetNotificationHistoryQuery(userId, type, startDate, endDate, page, pageSize);
        var notifications = await _historyQueryService.HandleAsync(query) ?? Enumerable.Empty<NotificationAggregate>();

        var resources = notifications.Select(n => NotificationResourceAssembler.ToResource(n));

        return Ok(resources);
    }

    [HttpPost("{notificationId}/read")]
    public async Task<IActionResult> MarkAsRead(string notificationId)
    {
        try
        {
            // Creamos la query por ID
            var query = new GetNotificationByIdQuery(notificationId);

            // Llamamos al QueryService
            var notification = await _historyQueryService.HandleAsync(query);

            if (notification == null)
                return NotFound(new { error = "Notification not found." });

            notification.MarkAsRead();

            // Actualizamos la notificación
            await _sendCommandService.UpdateAsync(notification);

            return Ok(new { message = "Notification marked as read" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

