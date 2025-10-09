using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftFocusBackend.Library.Application.ACL.Services;
using SoftFocusBackend.Library.Application.Internal.CommandServices;
using SoftFocusBackend.Library.Application.Internal.QueryServices;
using SoftFocusBackend.Library.Domain.Model.Commands;
using SoftFocusBackend.Library.Domain.Model.Queries;
using SoftFocusBackend.Library.Domain.Model.ValueObjects;
using SoftFocusBackend.Library.Interfaces.REST.Resources;
using SoftFocusBackend.Users.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Library.Interfaces.REST.Controllers;

[ApiController]
[Route("api/library/assignments")]
[Authorize]
public class AssignmentsController : ControllerBase
{
    private readonly IAssignmentCommandService _assignmentCommand;
    private readonly ICompletionCommandService _completionCommand;
    private readonly IAssignedContentQueryService _assignmentQuery;
    private readonly IUserIntegrationService _userIntegration;

    public AssignmentsController(
        IAssignmentCommandService assignmentCommand,
        ICompletionCommandService completionCommand,
        IAssignedContentQueryService assignmentQuery,
        IUserIntegrationService userIntegration)
    {
        _assignmentCommand = assignmentCommand;
        _completionCommand = completionCommand;
        _assignmentQuery = assignmentQuery;
        _userIntegration = userIntegration;
    }

    /// <summary>
    /// Psicólogo asigna contenido a uno o más pacientes
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> AssignContent([FromBody] AssignmentRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        // Validar que sea Psicólogo
        var userType = await _userIntegration.GetUserTypeAsync(userId);
        if (userType != UserType.Psychologist)
            return Forbid("Solo los psicólogos pueden asignar contenido");

        if (!Enum.TryParse<ContentType>(request.ContentType, true, out var contentType))
            return BadRequest("Invalid ContentType");

        var command = new AssignContentCommand(
            userId,
            request.PatientIds,
            request.ContentId,
            contentType,
            request.Notes
        );

        try
        {
            var assignmentIds = await _assignmentCommand.AssignContentAsync(command);

            return Created("/api/library/assignments", new
            {
                assignmentIds,
                message = $"Contenido asignado a {assignmentIds.Count} paciente(s)",
                totalAssigned = assignmentIds.Count
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    /// <summary>
    /// Paciente obtiene su contenido asignado
    /// </summary>
    [HttpGet("assigned")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetAssignedContent(
        [FromQuery] bool? completed = null)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        // Validar que NO sea Psicólogo
        var userType = await _userIntegration.GetUserTypeAsync(userId);
        if (userType == UserType.Psychologist)
            return Forbid("Los psicólogos no pueden ver contenido asignado");

        var query = new GetAssignedContentQuery(userId, completed);
        var assignments = await _assignmentQuery.GetAssignedContentAsync(query);

        var response = assignments.Select(a => new
        {
            assignmentId = a.Id,
            content = MapToContentResponse(a.Content),
            psychologistId = a.PsychologistId,
            assignedAt = a.AssignedAt,
            notes = a.Notes,
            isCompleted = a.IsCompleted,
            completedAt = a.CompletedAt
        }).ToList();

        return Ok(new
        {
            assignments = response,
            total = response.Count,
            pending = response.Count(a => !a.isCompleted),
            completed = response.Count(a => a.isCompleted)
        });
    }

    /// <summary>
    /// Paciente marca una asignación como completada
    /// </summary>
    [HttpPost("assigned/{assignmentId}/complete")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CompleteAssignment(string assignmentId)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var command = new MarkAsCompletedCommand(userId, assignmentId);

        try
        {
            await _completionCommand.MarkAsCompletedAsync(command);

            return Ok(new
            {
                message = "Asignación completada",
                assignmentId,
                completedAt = DateTime.UtcNow
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    /// <summary>
    /// Psicólogo obtiene todas las asignaciones que ha creado
    /// </summary>
    [HttpGet("by-psychologist")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetAssignmentsByPsychologist(
        [FromQuery] string? patientId = null)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        // Validar que sea Psicólogo
        var userType = await _userIntegration.GetUserTypeAsync(userId);
        if (userType != UserType.Psychologist)
            return Forbid("Solo los psicólogos pueden ver esta información");

        var assignments = await _assignmentQuery.GetAssignmentsByPsychologistAsync(userId, patientId);

        var response = assignments.Select(a => new
        {
            assignmentId = a.Id,
            patientId = a.PatientId,
            content = MapToContentResponse(a.Content),
            assignedAt = a.AssignedAt,
            notes = a.Notes,
            isCompleted = a.IsCompleted,
            completedAt = a.CompletedAt
        }).ToList();

        return Ok(new
        {
            assignments = response,
            total = response.Count,
            pending = response.Count(a => !a.isCompleted),
            completed = response.Count(a => a.isCompleted)
        });
    }

    private static ContentItemResponse MapToContentResponse(Domain.Model.Aggregates.ContentItem item)
    {
        return new ContentItemResponse
        {
            Id = item.ExternalId,
            Type = item.ContentType.ToString(),
            Title = item.Metadata.Title,
            PosterUrl = item.Metadata.PosterUrl,
            BackdropUrl = item.Metadata.BackdropUrl,
            Rating = item.Metadata.Rating,
            Duration = item.Metadata.Duration,
            Overview = item.Metadata.Overview,
            TrailerUrl = item.Metadata.TrailerUrl,
            EmotionalTags = item.EmotionalTags.Select(t => t.ToString()).ToList(),
            ExternalUrl = item.ExternalUrl,
            Artist = item.Metadata.Artist,
            Album = item.Metadata.Album,
            PreviewUrl = item.Metadata.PreviewUrl,
            SpotifyUrl = item.Metadata.SpotifyUrl,
            ChannelName = item.Metadata.ChannelName,
            YouTubeUrl = item.Metadata.YouTubeUrl,
            ThumbnailUrl = item.Metadata.ThumbnailUrl,
            Category = item.Metadata.Category,
            Address = item.Metadata.Address,
            Latitude = item.Metadata.Latitude != 0 ? item.Metadata.Latitude : null,
            Longitude = item.Metadata.Longitude != 0 ? item.Metadata.Longitude : null,
            Distance = item.Metadata.Distance > 0 ? item.Metadata.Distance : null,
            PhotoUrl = item.Metadata.PhotoUrl
        };
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst("user_id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
