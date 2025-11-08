using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MongoDB.Bson;
using MongoDB.Driver;
using SoftFocusBackend.Notification.Application.Internal.CommandServices;
using SoftFocusBackend.Notification.Application.Internal.QueryServices;
using SoftFocusBackend.Notification.Domain.Model.Commands;
using SoftFocusBackend.Notification.Domain.Model.Queries;
using SoftFocusBackend.Notification.Interfaces.REST.Resources;
using SoftFocusBackend.Notification.Interfaces.REST.Transform;
using SoftFocusBackend.Shared.Infrastructure.Persistence.MongoDB.Context;

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
    private readonly MongoDbContext _context;

    public NotificationController(
        SendNotificationCommandService sendCommandService,
        NotificationHistoryQueryService historyQueryService,
        PreferenceQueryService preferenceQueryService,
        MongoDbContext context)
    {
        _sendCommandService = sendCommandService;
        _historyQueryService = historyQueryService;
        _preferenceQueryService = preferenceQueryService;
        _context = context;
    }

    // GET: api/v1/notifications - Obtener todas las notificaciones del usuario actual
    [HttpGet]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] string? status = null,
        [FromQuery] string? type = null,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { error = "User not authenticated" });

            var query = new GetNotificationHistoryQuery(userId, type, null, null, page, size);
            var notifications = await _historyQueryService.HandleAsync(query) ?? Enumerable.Empty<NotificationAggregate>();

            var resources = notifications.Select(NotificationResourceAssembler.ToResource);
            
            return Ok(new NotificationListResponse 
            { 
                Notifications = resources.ToList(),
                TotalCount = notifications.Count(),
                Page = page,
                PageSize = size
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // GET: api/v1/notifications/{userId} - Obtener notificaciones de usuario específico (para admin)
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetNotificationsByUserId(
        string userId,
        [FromQuery] string? status = null,
        [FromQuery] string? type = null,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20)
    {
        try
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized(new { error = "User not authenticated" });

            var query = new GetNotificationHistoryQuery(userId, type, null, null, page, size);
            var notifications = await _historyQueryService.HandleAsync(query) ?? Enumerable.Empty<NotificationAggregate>();

            var resources = notifications.Select(NotificationResourceAssembler.ToResource);
            
            return Ok(new NotificationListResponse 
            { 
                Notifications = resources.ToList(),
                TotalCount = notifications.Count(),
                Page = page,
                PageSize = size
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // GET: api/v1/notifications/detail/{notificationId} - Obtener notificación específica
    [HttpGet("detail/{notificationId}")]
    public async Task<IActionResult> GetNotificationById(string notificationId)
    {
        try
        {
            var query = new GetNotificationByIdQuery(notificationId);
            var notification = await _historyQueryService.HandleAsync(query);

            if (notification == null)
                return NotFound(new { error = "Notification not found" });

            var resource = NotificationResourceAssembler.ToResource(notification);
            return Ok(resource);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // POST: api/v1/notifications/{notificationId}/read - Marcar como leída
    [HttpPost("{notificationId}/read")]
    public async Task<IActionResult> MarkAsRead(string notificationId)
    {
        try
        {
            var query = new GetNotificationByIdQuery(notificationId);
            var notification = await _historyQueryService.HandleAsync(query);

            if (notification == null)
                return NotFound(new { error = "Notification not found" });

            notification.MarkAsRead();
            await _sendCommandService.UpdateAsync(notification);

            return Ok(new { message = "Notification marked as read", notificationId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // POST: api/v1/notifications/read-all - Marcar todas como leídas
    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { error = "User not authenticated" });

            var unreadQuery = new GetUnreadNotificationsQuery(userId);
            var unreadNotifications = await _historyQueryService.HandleAsync(unreadQuery) ?? Enumerable.Empty<NotificationAggregate>();

            foreach (var notification in unreadNotifications)
            {
                notification.MarkAsRead();
                await _sendCommandService.UpdateAsync(notification);
            }

            return Ok(new { 
                message = "All notifications marked as read", 
                count = unreadNotifications.Count() 
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // DELETE: api/v1/notifications/{notificationId} - Eliminar notificación
    [HttpDelete("{notificationId}")]
    public async Task<IActionResult> DeleteNotification(string notificationId)
    {
        try
        {
            var query = new GetNotificationByIdQuery(notificationId);
            var notification = await _historyQueryService.HandleAsync(query);

            if (notification == null)
                return NotFound(new { error = "Notification not found" });

            await _sendCommandService.DeleteAsync(notification);
            
            return Ok(new { 
                message = "Notification deleted successfully", 
                notificationId 
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // GET: api/v1/notifications/unread-count - Obtener contador de no leídas
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { error = "User not authenticated" });

            var query = new GetUnreadNotificationsQuery(userId);
            var unreadNotifications = await _historyQueryService.HandleAsync(query) ?? Enumerable.Empty<NotificationAggregate>();

            return Ok(new UnreadCountResponse { UnreadCount = unreadNotifications.Count() });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // POST: api/v1/notifications - Crear nueva notificación
    [HttpPost]
    public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequest request)
    {
        try
        {
            // 🔧 CORRECCIÓN: Usar el userId del token, NO del request
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { error = "User not authenticated" });

            var command = new SendNotificationCommand(
                userId, // ← Usar el userId del token
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

            var resource = NotificationResourceAssembler.ToResource(notification as NotificationAggregate);
            return Ok(resource);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }
}

