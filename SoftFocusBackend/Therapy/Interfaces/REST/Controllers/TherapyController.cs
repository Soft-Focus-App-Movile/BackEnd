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
    [Route("api/therapy")]
    [Authorize]
    public class TherapyController : ControllerBase
    {
        private readonly EstablishConnectionCommandService _establishService;
        private readonly PatientDirectoryQueryService _directoryService;

        public TherapyController(
            EstablishConnectionCommandService establishService,
            PatientDirectoryQueryService directoryService)
        {
            _establishService = establishService;
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
            // Validate user is psychologist via ACL or claims

            var query = new GetPatientDirectoryQuery { PsychologistId = psychologistId };
            var patients = await _directoryService.Handle(query);
            return Ok(patients);
        }

        private string GetCurrentUserId()
        {
            return User.FindFirst("user_id")?.Value;
        }
    }
}