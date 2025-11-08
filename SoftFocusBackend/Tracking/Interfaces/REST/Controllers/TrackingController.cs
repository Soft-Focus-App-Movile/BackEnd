using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftFocusBackend.Tracking.Domain.Model.Aggregates;
using SoftFocusBackend.Tracking.Domain.Model.Commands;
using SoftFocusBackend.Tracking.Domain.Model.Queries;
using SoftFocusBackend.Tracking.Domain.Services;
using SoftFocusBackend.Tracking.Interfaces.REST.Resources;
using SoftFocusBackend.Tracking.Interfaces.REST.Transform;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using SoftFocusBackend.Library.Application.ACL.Services;
using SoftFocusBackend.Users.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Tracking.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/tracking")]
[Produces("application/json")]
[Authorize]
public class TrackingController : ControllerBase
{
    private readonly ICheckInCommandService _checkInCommandService;
    private readonly ICheckInQueryService _checkInQueryService;
    private readonly IEmotionalCalendarCommandService _emotionalCalendarCommandService;
    private readonly IEmotionalCalendarQueryService _emotionalCalendarQueryService;
    private readonly ILogger<TrackingController> _logger;
    private readonly IUserIntegrationService _userIntegrationService;

    public TrackingController(
        ICheckInCommandService checkInCommandService,
        ICheckInQueryService checkInQueryService,
        IEmotionalCalendarCommandService emotionalCalendarCommandService,
        IEmotionalCalendarQueryService emotionalCalendarQueryService,
        ILogger<TrackingController> logger,
        IUserIntegrationService userIntegrationService)
    {
        _checkInCommandService = checkInCommandService ?? throw new ArgumentNullException(nameof(checkInCommandService));
        _checkInQueryService = checkInQueryService ?? throw new ArgumentNullException(nameof(checkInQueryService));
        _emotionalCalendarCommandService = emotionalCalendarCommandService ?? throw new ArgumentNullException(nameof(emotionalCalendarCommandService));
        _emotionalCalendarQueryService = emotionalCalendarQueryService ?? throw new ArgumentNullException(nameof(emotionalCalendarQueryService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userIntegrationService = userIntegrationService ?? throw new ArgumentNullException(nameof(userIntegrationService));
    }

    /// <summary>
    /// Creates a daily check-in for the authenticated user
    /// </summary>
    /// <param name="resource">Check-in data including emotional level, energy level, mood description, sleep hours, symptoms, and notes</param>
    /// <returns>Created check-in with confirmation details</returns>
    /// <response code="201">Check-in created successfully</response>
    /// <response code="400">Invalid request data or user already completed check-in today</response>
    /// <response code="401">User not authenticated</response>
    [HttpPost("check-ins")]
    [SwaggerOperation(
        Summary = "Create daily check-in",
        Description = "Creates a new daily emotional and wellness check-in for the authenticated user. Users can only create one check-in per day.",
        OperationId = "CreateCheckIn",
        Tags = new[] { "Check-ins" }
    )]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateCheckIn([FromBody] CreateCheckInResource resource)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("Create check-in attempt without valid user ID");
                return Unauthorized(CheckInResourceAssembler.ToErrorResponse("Invalid user session"));
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid create check-in request for user: {UserId}", userId);
                return BadRequest(CheckInResourceAssembler.ToErrorResponse("Invalid request data"));
            }

            _logger.LogInformation("Create check-in request for user: {UserId}", userId);

            var command = CheckInResourceAssembler.ToCreateCommand(resource, userId);
            var checkIn = await _checkInCommandService.HandleCreateCheckInAsync(command);

            if (checkIn == null)
            {
                _logger.LogWarning("Failed to create check-in for user: {UserId}", userId);
                return BadRequest(CheckInResourceAssembler.ToErrorResponse("Failed to create check-in. You may have already completed today's check-in."));
            }

            var response = CheckInResourceAssembler.ToResource(checkIn);
            return CreatedAtAction(nameof(GetCheckIn), new { id = checkIn.Id }, 
                CheckInResourceAssembler.ToSuccessResponse(response, "Check-in created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating check-in");
            return StatusCode(StatusCodes.Status500InternalServerError,
                CheckInResourceAssembler.ToErrorResponse("An error occurred while creating check-in"));
        }
    }

    /// <summary>
    /// Retrieves a specific check-in by ID
    /// </summary>
    /// <param name="id">The unique identifier of the check-in</param>
    /// <returns>Check-in details if found and belongs to the authenticated user</returns>
    /// <response code="200">Check-in found and returned</response>
    /// <response code="404">Check-in not found or doesn't belong to user</response>
    /// <response code="401">User not authenticated</response>
    [HttpGet("check-ins/{id}")]
    [SwaggerOperation(
        Summary = "Get check-in by ID",
        Description = "Retrieves a specific check-in by its unique identifier. Users can only access their own check-ins.",
        OperationId = "GetCheckIn",
        Tags = new[] { "Check-ins" }
    )]
    [ProducesResponseType(typeof(CheckInResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCheckIn([FromRoute] string id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(CheckInResourceAssembler.ToErrorResponse("Invalid user session"));
            }

            _logger.LogInformation("Get check-in request: {CheckInId} for user: {UserId}", id, userId);

            var query = new GetCheckInByIdQuery(id);
            var checkIn = await _checkInQueryService.HandleGetCheckInByIdAsync(query);

            if (checkIn == null || checkIn.UserId != userId)
            {
                _logger.LogWarning("Check-in not found or access denied: {CheckInId} for user: {UserId}", id, userId);
                return NotFound(CheckInResourceAssembler.ToErrorResponse("Check-in not found"));
            }

            var response = CheckInResourceAssembler.ToResource(checkIn);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting check-in: {CheckInId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                CheckInResourceAssembler.ToErrorResponse("An error occurred while retrieving check-in"));
        }
    }

    /// <summary>
    /// Retrieves today's check-in for the authenticated user
    /// </summary>
    /// <returns>Today's check-in if completed, otherwise null</returns>
    /// <response code="200">Today's check-in returned (may be null if not completed)</response>
    /// <response code="401">User not authenticated</response>
    [HttpGet("check-ins/today")]
    [SwaggerOperation(
        Summary = "Get today's check-in",
        Description = "Retrieves the check-in completed today by the authenticated user. Returns null if no check-in has been completed today.",
        OperationId = "GetTodayCheckIn",
        Tags = new[] { "Check-ins" }
    )]
    [ProducesResponseType(typeof(CheckInResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTodayCheckIn()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(CheckInResourceAssembler.ToErrorResponse("Invalid user session"));
            }

            _logger.LogInformation("Get today check-in request for user: {UserId}", userId);

            var query = new GetTodayCheckInQuery(userId);
            var checkIn = await _checkInQueryService.HandleGetTodayCheckInAsync(query);

            var response = checkIn != null ? CheckInResourceAssembler.ToResource(checkIn) : null;
            return Ok(new { success = true, data = response, hasCompletedToday = checkIn != null, timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting today check-in");
            return StatusCode(StatusCodes.Status500InternalServerError,
                CheckInResourceAssembler.ToErrorResponse("An error occurred while retrieving today's check-in"));
        }
    }

    /// <summary>
    /// Retrieves user's check-in history with optional date filtering
    /// </summary>
    /// <param name="startDate">Optional start date for filtering (YYYY-MM-DD format)</param>
    /// <param name="endDate">Optional end date for filtering (YYYY-MM-DD format)</param>
    /// <param name="pageNumber">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 20, max: 100)</param>
    /// <returns>Paginated list of user's check-ins</returns>
    /// <response code="200">Check-ins retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    [HttpGet("check-ins")]
    [SwaggerOperation(
        Summary = "Get user's check-in history",
        Description = "Retrieves the authenticated user's check-in history with optional date range filtering and pagination support.",
        OperationId = "GetUserCheckIns",
        Tags = new[] { "Check-ins" }
    )]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserCheckIns(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(CheckInResourceAssembler.ToErrorResponse("Invalid user session"));
            }

            pageSize = Math.Min(pageSize, 100); // Limit page size
            _logger.LogInformation("Get user check-ins request for user: {UserId}, Page: {PageNumber}, Size: {PageSize}", userId, pageNumber, pageSize);

            var query = new GetUserCheckInsQuery(userId, startDate, endDate, pageNumber, pageSize);
            var checkIns = await _checkInQueryService.HandleGetUserCheckInsAsync(query);

            var resources = CheckInResourceAssembler.ToResourceList(checkIns);
            return Ok(TrackingResourceAssembler.ToPaginatedResponse(resources, pageNumber, pageSize, checkIns.Count));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user check-ins");
            return StatusCode(StatusCodes.Status500InternalServerError,
                CheckInResourceAssembler.ToErrorResponse("An error occurred while retrieving check-ins"));
        }
    }

    /// <summary>
    /// Creates an emotional calendar entry for a specific date
    /// </summary>
    /// <param name="resource">Calendar entry data including date, emoji, mood level, and emotional tags</param>
    /// <returns>Created emotional calendar entry</returns>
    /// <response code="201">Calendar entry created successfully</response>
    /// <response code="400">Invalid request data or entry already exists for the date</response>
    /// <response code="401">User not authenticated</response>
    [HttpPost("emotional-calendar")]
    [SwaggerOperation(
        Summary = "Create emotional calendar entry",
        Description = "Creates a new emotional calendar entry for a specific date. Users can only create one entry per date.",
        OperationId = "CreateEmotionalCalendarEntry",
        Tags = new[] { "Emotional Calendar" }
    )]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateEmotionalCalendarEntry([FromBody] CreateEmotionalCalendarEntryResource resource)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(EmotionalCalendarResourceAssembler.ToErrorResponse("Invalid user session"));
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid create emotional calendar entry request for user: {UserId}", userId);
                return BadRequest(EmotionalCalendarResourceAssembler.ToErrorResponse("Invalid request data"));
            }

            _logger.LogInformation("Create emotional calendar entry request for user: {UserId}, Date: {Date}", userId, resource.Date);

            var command = EmotionalCalendarResourceAssembler.ToCreateCommand(resource, userId);
            var entry = await _emotionalCalendarCommandService.HandleCreateEmotionalCalendarEntryAsync(command);

            if (entry == null)
            {
                _logger.LogWarning("Failed to create emotional calendar entry for user: {UserId}", userId);
                return BadRequest(EmotionalCalendarResourceAssembler.ToErrorResponse("Failed to create calendar entry. An entry may already exist for this date."));
            }

            var response = EmotionalCalendarResourceAssembler.ToResource(entry);
            return CreatedAtAction(nameof(GetEmotionalCalendarEntryByDate), 
                new { date = entry.Date.ToString("yyyy-MM-dd") },
                EmotionalCalendarResourceAssembler.ToSuccessResponse(response, "Emotional calendar entry created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating emotional calendar entry");
            return StatusCode(StatusCodes.Status500InternalServerError,
                EmotionalCalendarResourceAssembler.ToErrorResponse("An error occurred while creating calendar entry"));
        }
    }

    /// <summary>
    /// Retrieves emotional calendar entry for a specific date
    /// </summary>
    /// <param name="date">Date in YYYY-MM-DD format</param>
    /// <returns>Emotional calendar entry for the specified date</returns>
    /// <response code="200">Calendar entry found and returned</response>
    /// <response code="404">No entry found for the specified date</response>
    /// <response code="401">User not authenticated</response>
    [HttpGet("emotional-calendar/{date}")]
    [SwaggerOperation(
        Summary = "Get emotional calendar entry by date",
        Description = "Retrieves the emotional calendar entry for a specific date. Users can only access their own entries.",
        OperationId = "GetEmotionalCalendarEntryByDate",
        Tags = new[] { "Emotional Calendar" }
    )]
    [ProducesResponseType(typeof(EmotionalCalendarResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetEmotionalCalendarEntryByDate([FromRoute] string date)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(EmotionalCalendarResourceAssembler.ToErrorResponse("Invalid user session"));
            }

            if (!DateTime.TryParseExact(date, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var parsedDate))
            {
                return BadRequest(EmotionalCalendarResourceAssembler.ToErrorResponse("Invalid date format. Use YYYY-MM-DD."));
            }

            // Normalize to UTC date only
            parsedDate = DateTime.SpecifyKind(parsedDate.Date, DateTimeKind.Utc);

            _logger.LogInformation("Get emotional calendar entry request for user: {UserId}, Date: {Date}, ParsedDate: {ParsedDate}", userId, date, parsedDate);

            var query = new GetEmotionalCalendarEntryByDateQuery(userId, parsedDate);
            var entry = await _emotionalCalendarQueryService.HandleGetEmotionalCalendarEntryByDateAsync(query);

            if (entry == null)
            {
                return NotFound(EmotionalCalendarResourceAssembler.ToErrorResponse("No calendar entry found for this date"));
            }

            var response = EmotionalCalendarResourceAssembler.ToResource(entry);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting emotional calendar entry by date: {Date}", date);
            return StatusCode(StatusCodes.Status500InternalServerError,
                EmotionalCalendarResourceAssembler.ToErrorResponse("An error occurred while retrieving calendar entry"));
        }
    }

    /// <summary>
    /// Retrieves user's emotional calendar with optional date range filtering
    /// </summary>
    /// <param name="startDate">Optional start date for filtering (YYYY-MM-DD format)</param>
    /// <param name="endDate">Optional end date for filtering (YYYY-MM-DD format)</param>
    /// <returns>List of emotional calendar entries</returns>
    /// <response code="200">Calendar entries retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    [HttpGet("emotional-calendar")]
    [SwaggerOperation(
        Summary = "Get user's emotional calendar",
        Description = "Retrieves the authenticated user's emotional calendar entries with optional date range filtering.",
        OperationId = "GetUserEmotionalCalendar",
        Tags = new[] { "Emotional Calendar" }
    )]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserEmotionalCalendar(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(EmotionalCalendarResourceAssembler.ToErrorResponse("Invalid user session"));
            }

            _logger.LogInformation("Get user emotional calendar request for user: {UserId}, StartDate: {StartDate}, EndDate: {EndDate}", userId, startDate, endDate);

            List<EmotionalCalendar> entries;

            if (startDate.HasValue || endDate.HasValue)
            {
                // Normalize dates to UTC and date only
                var normalizedStartDate = startDate?.Date ?? DateTime.MinValue;
                var normalizedEndDate = endDate?.Date ?? DateTime.MaxValue.Date;
                
                normalizedStartDate = DateTime.SpecifyKind(normalizedStartDate, DateTimeKind.Utc);
                normalizedEndDate = DateTime.SpecifyKind(normalizedEndDate, DateTimeKind.Utc);

                var rangeQuery = new GetEmotionalCalendarByDateRangeQuery(userId, normalizedStartDate, normalizedEndDate);
                entries = await _emotionalCalendarQueryService.HandleGetEmotionalCalendarByDateRangeAsync(rangeQuery);
            }
            else
            {
                var query = new GetUserEmotionalCalendarQuery(userId);
                entries = await _emotionalCalendarQueryService.HandleGetUserEmotionalCalendarAsync(query);
            }

            var resources = EmotionalCalendarResourceAssembler.ToResourceList(entries);
            return Ok(EmotionalCalendarResourceAssembler.ToCalendarResponse(resources, startDate, endDate));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user emotional calendar");
            return StatusCode(StatusCodes.Status500InternalServerError,
                EmotionalCalendarResourceAssembler.ToErrorResponse("An error occurred while retrieving emotional calendar"));
        }
    }

    /// <summary>
    /// Retrieves comprehensive tracking dashboard with insights and statistics
    /// </summary>
    /// <param name="days">Number of days to include in the analysis (default: 30, max: 365)</param>
    /// <returns>Dashboard data with tracking summary, statistics, and personalized insights</returns>
    /// <response code="200">Dashboard data retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    [HttpGet("dashboard")]
    [SwaggerOperation(
        Summary = "Get tracking dashboard",
        Description = "Retrieves comprehensive tracking dashboard with statistics, insights, and trends for the authenticated user.",
        OperationId = "GetTrackingDashboard",
        Tags = new[] { "Dashboard" }
    )]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTrackingDashboard([FromQuery] int days = 30)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(TrackingResourceAssembler.ToPaginatedResponse(new List<object>(), 1, 1, 0));
            }

            days = Math.Min(days, 365); // Limit to 1 year
            var startDate = DateTime.UtcNow.AddDays(-days);
            var endDate = DateTime.UtcNow;

            _logger.LogInformation("Get tracking dashboard request for user: {UserId}, Days: {Days}", userId, days);

            // Get check-ins
            var checkInsQuery = new GetUserCheckInsQuery(userId, startDate, endDate, 1, 1000);
            var checkIns = await _checkInQueryService.HandleGetUserCheckInsAsync(checkInsQuery);

            // Get emotional calendar entries
            var calendarQuery = new GetEmotionalCalendarByDateRangeQuery(userId, startDate, endDate);
            var calendarEntries = await _emotionalCalendarQueryService.HandleGetEmotionalCalendarByDateRangeAsync(calendarQuery);

            // Get today's check-in
            var todayQuery = new GetTodayCheckInQuery(userId);
            var todayCheckIn = await _checkInQueryService.HandleGetTodayCheckInAsync(todayQuery);

            var summary = TrackingResourceAssembler.ToSummaryResource(checkIns, calendarEntries, todayCheckIn);
            return Ok(TrackingResourceAssembler.ToTrackingDashboard(summary));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tracking dashboard");
            return StatusCode(StatusCodes.Status500InternalServerError,
                TrackingResourceAssembler.ToPaginatedResponse(new List<object>(), 1, 1, 0));
        }
    }
    
    /// <summary>
    /// Retrieves comprehensive tracking dashboard for a specific patient
    /// </summary>
    /// <param name="userId">The unique identifier of the patient</param>
    /// <param name="days">Number of days to include in the analysis (default: 30, max: 365)</param>
    /// <returns>Dashboard data with tracking summary, statistics, and personalized insights for the patient</returns>
    /// <response code="200">Dashboard data retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User is not a psychologist</response>
    /// <response code="404">Patient user not found</response>
    [HttpGet("dashboard/{userId}")]
    [SwaggerOperation(
        Summary = "Get patient's tracking dashboard",
        Description = "Retrieves comprehensive tracking dashboard for a specific patient. Only accessible by authenticated psychologists.",
        OperationId = "GetPatientTrackingDashboard",
        Tags = new[] { "Dashboard" }
    )]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPatientTrackingDashboard([FromRoute] string userId, [FromQuery] int days = 30)
    {
        try
        {
            // 1. Validar al usuario que hace la llamada (el psicólogo)
            var callerId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(callerId))
            {
                // CORRECCIÓN: Usar CheckInResourceAssembler
                return Unauthorized(CheckInResourceAssembler.ToErrorResponse("Invalid user session"));
            }

            // 2. Verificar que el llamante sea un psicólogo
            var callerUserType = await _userIntegrationService.GetUserTypeAsync(callerId);
            if (callerUserType != UserType.Psychologist)
            {
                _logger.LogWarning("Forbidden: Non-psychologist user {CallerId} attempted to access dashboard for {UserId}", callerId, userId);
                return Forbid("Only psychologists can access patient dashboards.");
            }
            
            // 3. Validar que el 'userId' (paciente) exista
            try
            {
                // CORRECCIÓN: Eliminada la comprobación 'UserType.Unknown'.
                // Simplemente llamamos al servicio. Si no existe, el 'catch' lo manejará.
                await _userIntegrationService.GetUserTypeAsync(userId);
            }
            catch (Exception ex)
            {
                // Asumimos que una excepción aquí significa que el usuario no fue encontrado.
                _logger.LogWarning(ex, "Failed to retrieve patient type for {UserId}, assuming not found.", userId);
                // CORRECCIÓN: Usar CheckInResourceAssembler
                return NotFound(CheckInResourceAssembler.ToErrorResponse("Patient not found"));
            }

            // 4. Reutilizar la lógica del dashboard, pero usando el 'userId' del paciente
            days = Math.Min(days, 365); // Limitar a 1 año
            var startDate = DateTime.UtcNow.AddDays(-days);
            var endDate = DateTime.UtcNow;

            _logger.LogInformation("Get tracking dashboard request for patient: {UserId} by psychologist: {CallerId}, Days: {Days}", userId, callerId, days);

            // Obtener check-ins PARA EL PACIENTE
            var checkInsQuery = new GetUserCheckInsQuery(userId, startDate, endDate, 1, 1000);
            var checkIns = await _checkInQueryService.HandleGetUserCheckInsAsync(checkInsQuery);

            // Obtener calendario emocional PARA EL PACIENTE
            var calendarQuery = new GetEmotionalCalendarByDateRangeQuery(userId, startDate, endDate);
            var calendarEntries = await _emotionalCalendarQueryService.HandleGetEmotionalCalendarByDateRangeAsync(calendarQuery);

            // Obtener check-in de hoy PARA EL PACIENTE
            var todayQuery = new GetTodayCheckInQuery(userId);
            var todayCheckIn = await _checkInQueryService.HandleGetTodayCheckInAsync(todayQuery);

            // Generar el resumen
            var summary = TrackingResourceAssembler.ToSummaryResource(checkIns, calendarEntries, todayCheckIn);
            
            return Ok(TrackingResourceAssembler.ToTrackingDashboard(summary));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tracking dashboard for patient: {UserId}", userId);
            // CORRECCIÓN: Usar CheckInResourceAssembler
            return StatusCode(StatusCodes.Status500InternalServerError,
                CheckInResourceAssembler.ToErrorResponse("An error occurred while retrieving tracking dashboard"));
        }
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst("user_id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}