using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftFocusBackend.Crisis.Application.Internal.CommandServices;
using SoftFocusBackend.Crisis.Application.Internal.QueryServices;
using SoftFocusBackend.Crisis.Domain.Model.Queries;
using SoftFocusBackend.Crisis.Domain.Model.ValueObjects;
using SoftFocusBackend.Crisis.Interfaces.REST.Resources;
using SoftFocusBackend.Crisis.Interfaces.REST.Transform;

namespace SoftFocusBackend.Crisis.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/crisis")]
[Produces("application/json")]
[Authorize]
public class CrisisAlertController : ControllerBase
{
    private readonly ICrisisAlertCommandService _commandService;
    private readonly ICrisisAlertQueryService _queryService;
    private readonly ILogger<CrisisAlertController> _logger;
    private readonly CrisisAlertResourceFromEntityAssembler _assembler;

    public CrisisAlertController(
        ICrisisAlertCommandService commandService,
        ICrisisAlertQueryService queryService,
        ILogger<CrisisAlertController> logger,
        CrisisAlertResourceFromEntityAssembler assembler)
    {
        _commandService = commandService;
        _queryService = queryService;
        _logger = logger;
        _assembler = assembler;
    }

    [HttpPost("alert")]
    [ProducesResponseType(typeof(CrisisAlertResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateCrisisAlert([FromBody] CreateCrisisAlertResource resource)
    {
        try
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new { error = "User not authenticated" });
            }

            _logger.LogWarning("CRISIS ALERT: User {UserId} pressed crisis button", userId);

            var command = CreateCrisisAlertCommandFromResourceAssembler.ToCommandFromResource(resource, userId);
            var alert = await _commandService.Handle(command);

            var responseResource = await _assembler.ToResourceFromEntity(alert);

            return CreatedAtAction(nameof(GetAlertById), new { id = alert.Id }, responseResource);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error creating crisis alert");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating crisis alert");
            return StatusCode(500, new { error = "An error occurred while creating the crisis alert" });
        }
    }

    [HttpGet("alerts")]
    [ProducesResponseType(typeof(IEnumerable<CrisisAlertResource>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPsychologistAlerts(
        [FromQuery] string? severity = null,
        [FromQuery] string? status = null,
        [FromQuery] int? limit = null)
    {
        try
        {
            var psychologistId = GetUserId();
            if (string.IsNullOrWhiteSpace(psychologistId))
            {
                return Unauthorized(new { error = "User not authenticated" });
            }

            AlertSeverity? severityEnum = null;
            if (!string.IsNullOrWhiteSpace(severity))
            {
                severityEnum = Enum.Parse<AlertSeverity>(severity, ignoreCase: true);
            }

            AlertStatus? statusEnum = null;
            if (!string.IsNullOrWhiteSpace(status))
            {
                statusEnum = Enum.Parse<AlertStatus>(status, ignoreCase: true);
            }

            var query = new GetPsychologistAlertsQuery(psychologistId, severityEnum, statusEnum, limit);
            var alerts = await _queryService.Handle(query);

            var resources = await _assembler.ToResourceFromEntityList(alerts);

            return Ok(resources);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving psychologist alerts");
            return StatusCode(500, new { error = "An error occurred while retrieving alerts" });
        }
    }

    [HttpGet("alerts/{id}")]
    [ProducesResponseType(typeof(CrisisAlertResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAlertById(string id)
    {
        try
        {
            var query = new GetAlertByIdQuery(id);
            var alert = await _queryService.Handle(query);

            if (alert == null)
            {
                return NotFound(new { error = $"Alert with id {id} not found" });
            }

            var resource = await _assembler.ToResourceFromEntity(alert);

            return Ok(resource);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving alert by id");
            return StatusCode(500, new { error = "An error occurred while retrieving the alert" });
        }
    }

    [HttpPut("alerts/{id}/status")]
    [ProducesResponseType(typeof(CrisisAlertResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateAlertStatus(string id, [FromBody] UpdateAlertStatusResource resource)
    {
        try
        {
            var command = UpdateAlertStatusCommandFromResourceAssembler.ToCommandFromResource(resource, id);
            var alert = await _commandService.Handle(command);

            var responseResource = await _assembler.ToResourceFromEntity(alert);

            return Ok(responseResource);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error updating alert status");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating alert status");
            return StatusCode(500, new { error = "An error occurred while updating the alert status" });
        }
    }

    [HttpPut("alerts/{id}/severity")]
    [ProducesResponseType(typeof(CrisisAlertResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateAlertSeverity(string id, [FromBody] UpdateAlertSeverityResource resource)
    {
        try
        {
            var command = UpdateAlertSeverityCommandFromResourceAssembler.ToCommandFromResource(resource, id);
            var alert = await _commandService.Handle(command);

            var responseResource = await _assembler.ToResourceFromEntity(alert);

            return Ok(responseResource);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error updating alert severity");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating alert severity");
            return StatusCode(500, new { error = "An error occurred while updating the alert severity" });
        }
    }

    [HttpGet("alerts/count/pending")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPendingAlertCount()
    {
        try
        {
            var psychologistId = GetUserId();
            if (string.IsNullOrWhiteSpace(psychologistId))
            {
                return Unauthorized(new { error = "User not authenticated" });
            }

            var count = await _queryService.GetPendingAlertCount(psychologistId);

            return Ok(new { count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving pending alert count");
            return StatusCode(500, new { error = "An error occurred while retrieving the count" });
        }
    }

    private string? GetUserId()
    {
        return User.FindFirst("user_id")?.Value;
    }
}
