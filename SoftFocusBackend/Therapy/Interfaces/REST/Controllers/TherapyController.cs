using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftFocusBackend.Therapy.Application.Internal.CommandServices;
using SoftFocusBackend.Therapy.Application.Internal.QueryServices;
using SoftFocusBackend.Therapy.Domain.Model.Aggregates;
using SoftFocusBackend.Therapy.Domain.Model.Commands;
using SoftFocusBackend.Therapy.Domain.Model.Queries;
using SoftFocusBackend.Therapy.Interfaces.REST.Resources;

namespace SoftFocusBackend.Therapy.Interfaces.REST.Controllers
{
    [ApiController]
    [Route("api/v1/therapy")]
    [Authorize]
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
        public async Task<IActionResult> GetPatientDirectory()
        {
            var psychologistId = GetCurrentUserId();

            var query = new GetPatientDirectoryQuery { PsychologistId = psychologistId };
            var patients = await _directoryService.Handle(query);
            return Ok(patients);
        }

        [HttpGet("my-relationship")]
        public async Task<IActionResult> GetMyRelationship()
        {
            var patientId = GetCurrentUserId();
            var query = new GetMyRelationshipQuery { PatientId = patientId };
            var relationship = await _directoryService.GetMyRelationship(query);

            if (relationship == null)
                return Ok(new { hasRelationship = false, relationship = (object)null });

            return Ok(new { hasRelationship = true, relationship });
        }

        [HttpDelete("disconnect/{relationshipId}")]
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