using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftFocusBackend.Therapy.Application.Internal.CommandServices;
using SoftFocusBackend.Therapy.Application.Internal.QueryServices;
using SoftFocusBackend.Therapy.Domain.Model.Aggregates;
using SoftFocusBackend.Therapy.Domain.Model.Commands;
using SoftFocusBackend.Therapy.Interfaces.REST.Resources;
using Swashbuckle.AspNetCore.Annotations;

namespace SoftFocusBackend.Therapy.Interfaces.REST.Controllers
{
    [ApiController]
    [Route("api/v1/therapy/tasks")]
    [Authorize]
    [Produces("application/json")]
    public class PatientTaskController : ControllerBase
    {
        private readonly PatientTaskCommandService _commandService;
        private readonly PatientTaskQueryService _queryService;

        public PatientTaskController(
            PatientTaskCommandService commandService,
            PatientTaskQueryService queryService)
        {
            _commandService = commandService;
            _queryService = queryService;
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Assign a custom task to a patient",
            Description = "Allows a psychologist to write and assign a free-text task/purpose to one of their patients. Only works if an active therapeutic relationship exists.",
            OperationId = "CreatePatientTask",
            Tags = new[] { "Patient Tasks" }
        )]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateTask([FromBody] CreatePatientTaskRequest request)
        {
            var psychologistId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(psychologistId))
                return Unauthorized(new { error = "User not authenticated" });

            try
            {
                var command = new CreatePatientTaskCommand
                {
                    PsychologistId = psychologistId,
                    PatientId = request.PatientId,
                    Title = request.Title,
                    Description = request.Description
                };

                var task = await _commandService.Handle(command);
                return Ok(ToResource(task));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
            }
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Get tasks assigned to a patient (psychologist view)",
            Description = "Retrieves the custom tasks the authenticated psychologist assigned to a specific patient.",
            OperationId = "GetPatientTasks",
            Tags = new[] { "Patient Tasks" }
        )]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTasks([FromQuery] string patientId)
        {
            var psychologistId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(psychologistId))
                return Unauthorized(new { error = "User not authenticated" });

            if (string.IsNullOrWhiteSpace(patientId))
                return BadRequest(new { error = "patientId is required" });

            var tasks = await _queryService.GetByPsychologistAndPatient(psychologistId, patientId);
            return Ok(tasks.Select(ToResource));
        }

        [HttpGet("assigned")]
        [SwaggerOperation(
            Summary = "Get my assigned tasks (patient view)",
            Description = "Retrieves the custom tasks assigned to the authenticated patient.",
            OperationId = "GetMyPatientTasks",
            Tags = new[] { "Patient Tasks" }
        )]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyTasks()
        {
            var patientId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(patientId))
                return Unauthorized(new { error = "User not authenticated" });

            var tasks = await _queryService.GetForPatient(patientId);
            return Ok(tasks.Select(ToResource));
        }

        [HttpPost("{taskId}/complete")]
        [SwaggerOperation(
            Summary = "Mark a task as completed",
            Description = "Allows the patient to mark one of their assigned tasks as completed.",
            OperationId = "CompletePatientTask",
            Tags = new[] { "Patient Tasks" }
        )]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CompleteTask(string taskId)
        {
            var patientId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(patientId))
                return Unauthorized(new { error = "User not authenticated" });

            try
            {
                var command = new CompletePatientTaskCommand { TaskId = taskId, PatientId = patientId };
                var task = await _commandService.HandleComplete(command);
                return Ok(ToResource(task));
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
            }
        }

        private static object ToResource(PatientTask task) => new
        {
            id = task.Id,
            psychologistId = task.PsychologistId,
            patientId = task.PatientId,
            title = task.Title,
            description = task.Description,
            isCompleted = task.IsCompleted,
            completedAt = task.CompletedAt,
            assignedAt = task.AssignedAt,
            createdAt = task.CreatedAt
        };

        private string GetCurrentUserId()
        {
            return User.FindFirst("user_id")?.Value;
        }
    }
}
