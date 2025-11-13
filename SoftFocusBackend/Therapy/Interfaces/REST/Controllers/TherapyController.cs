using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftFocusBackend.Therapy.Application.Internal.CommandServices;
using SoftFocusBackend.Therapy.Application.Internal.QueryServices;
using SoftFocusBackend.Therapy.Domain.Model.Aggregates;
using SoftFocusBackend.Therapy.Domain.Model.Commands;
using SoftFocusBackend.Therapy.Domain.Model.Queries;
using SoftFocusBackend.Therapy.Interfaces.REST.Resources;
using Swashbuckle.AspNetCore.Annotations;

namespace SoftFocusBackend.Therapy.Interfaces.REST.Controllers
{
    [ApiController]
    [Route("api/v1/therapy")]
    [Authorize]
    [Produces("application/json")]
    public class TherapyController : ControllerBase
    {
        private readonly EstablishConnectionCommandService _establishService;
        private readonly TerminateRelationshipCommandService _terminateService;
        private readonly PatientDirectoryQueryService _directoryService;

        public TherapyController(
            EstablishConnectionCommandService establishService,
            TerminateRelationshipCommandService terminateService,
            PatientDirectoryQueryService directoryService)
        {
            _establishService = establishService;
            _terminateService = terminateService;
            _directoryService = directoryService;
        }

        [HttpPost("connect")]
        [SwaggerOperation(
            Summary = "Establish therapeutic relationship",
            Description = "Creates a new therapeutic relationship between a patient and psychologist using a connection code. Only patients can initiate connections.",
            OperationId = "EstablishConnection",
            Tags = new[] { "Therapy" }
        )]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> EstablishConnection([FromBody] EstablishConnectionRequest request)
        {
            var command = new EstablishConnectionCommand
            {
                PatientId = GetCurrentUserId(), // From Auth claims
                ConnectionCode = request.ConnectionCode
            };

            var relationship = await _establishService.Handle(command);
            return Ok(new { RelationshipId = relationship.Id });
        }

        [HttpGet("patients")]
        [SwaggerOperation(
            Summary = "Get psychologist's patient directory",
            Description = "Retrieves the list of patients connected to the authenticated psychologist. Only shows active relationships.",
            OperationId = "GetPatientDirectory",
            Tags = new[] { "Therapy" }
        )]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPatientDirectory()
        {
            var psychologistId = GetCurrentUserId();

            var query = new GetPatientDirectoryQuery { PsychologistId = psychologistId };
            var patients = await _directoryService.Handle(query);
            return Ok(patients);
        }

        [HttpGet("my-relationship")]
        [SwaggerOperation(
            Summary = "Get patient's active relationship",
            Description = "Retrieves the current active therapeutic relationship for the authenticated patient. Returns null if no active relationship exists.",
            OperationId = "GetMyRelationship",
            Tags = new[] { "Therapy" }
        )]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyRelationship()
        {
            var patientId = GetCurrentUserId();
            var query = new GetMyRelationshipQuery { PatientId = patientId };
            var relationship = await _directoryService.GetMyRelationship(query);

            if (relationship == null)
                return Ok(new { hasRelationship = false, relationship = (object)null });

            return Ok(new { hasRelationship = true, relationship });
        }

        [HttpGet("relationship-with/{patientId}")]
        [SwaggerOperation(
            Summary = "Get relationship with specific patient",
            Description = "Retrieves the active therapeutic relationship ID between the authenticated psychologist and a specific patient.",
            OperationId = "GetRelationshipWithPatient",
            Tags = new[] { "Therapy" }
        )]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetRelationshipWithPatient(string patientId)
        {
            var psychologistId = GetCurrentUserId();
            var query = new GetPsychologistRelationshipsQuery { PsychologistId = psychologistId };
            var relationships = await _directoryService.GetPsychologistRelationships(query);

            // Find the specific relationship with this patient
            var relationship = relationships.FirstOrDefault(r =>
            {
                // Assuming the relationship object has a patientId property
                var patientIdProp = r.GetType().GetProperty("patientId");
                return patientIdProp?.GetValue(r)?.ToString() == patientId;
            });

            if (relationship == null)
                return NotFound(new { message = "No active relationship found with this patient" });

            // Extract just the relationship ID
            var relationshipIdProp = relationship.GetType().GetProperty("id");
            var relationshipId = relationshipIdProp?.GetValue(relationship)?.ToString();

            return Ok(new { relationshipId, patientId });
        }

        [HttpDelete("disconnect/{relationshipId}")]
        [SwaggerOperation(
            Summary = "Terminate therapeutic relationship",
            Description = "Ends an active therapeutic relationship. Can be initiated by either the patient or psychologist. The relationship status will be set to Terminated.",
            OperationId = "TerminateRelationship",
            Tags = new[] { "Therapy" }
        )]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> TerminateRelationship(string relationshipId)
        {
            var userId = GetCurrentUserId();

            var command = new TerminateRelationshipCommand
            {
                UserId = userId,
                RelationshipId = relationshipId
            };

            try
            {
                await _terminateService.Handle(command);
                return Ok(new { message = "Relationship terminated successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst("user_id")?.Value;
        }
    }
}