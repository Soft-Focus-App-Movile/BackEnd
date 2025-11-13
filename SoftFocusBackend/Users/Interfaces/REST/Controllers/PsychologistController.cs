using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SoftFocusBackend.Users.Application.Internal.CommandServices;
using SoftFocusBackend.Users.Application.Internal.QueryServices;
using SoftFocusBackend.Users.Domain.Model.Commands;
using SoftFocusBackend.Users.Domain.Model.Queries;
using SoftFocusBackend.Users.Interfaces.REST.Resources;
using SoftFocusBackend.Users.Interfaces.REST.Transform;
using System.Security.Claims;
using SoftFocusBackend.Users.Domain.Services;
using SoftFocusBackend.Shared.Infrastructure.ExternalServices.Cloudinary.Configuration;
using SoftFocusBackend.Shared.Infrastructure.ExternalServices.Cloudinary.Services;
using SoftFocusBackend.Users.Application.Internal.OutboundServices;
using Swashbuckle.AspNetCore.Annotations;

namespace SoftFocusBackend.Users.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/users/psychologist")]
[Produces("application/json")]
[Authorize]
public class PsychologistController : ControllerBase
{
    private readonly IPsychologistCommandService _psychologistCommandService;
    private readonly IPsychologistQueryService _psychologistQueryService;
    private readonly ICloudinaryImageService _cloudinaryImageService;
    private readonly CloudinarySettings _cloudinarySettings;
    private readonly ILogger<PsychologistController> _logger;
    private readonly IUserFacade _userFacade;

    public PsychologistController(
        IPsychologistCommandService psychologistCommandService,
        IPsychologistQueryService psychologistQueryService,
        ICloudinaryImageService cloudinaryImageService,
        IOptions<CloudinarySettings> cloudinarySettings,
        ILogger<PsychologistController> logger,
        IUserFacade userFacade)
    {
        _psychologistCommandService = psychologistCommandService ?? throw new ArgumentNullException(nameof(psychologistCommandService));
        _psychologistQueryService = psychologistQueryService ?? throw new ArgumentNullException(nameof(psychologistQueryService));
        _cloudinaryImageService = cloudinaryImageService ?? throw new ArgumentNullException(nameof(cloudinaryImageService));
        _cloudinarySettings = cloudinarySettings.Value;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userFacade = userFacade ?? throw new ArgumentNullException(nameof(userFacade));
    }

    [HttpGet("verification")]
    [SwaggerOperation(
        Summary = "Get psychologist verification status",
        Description = "Retrieves the verification status and documentation of the authenticated psychologist.",
        OperationId = "GetVerificationStatus",
        Tags = new[] { "Psychologist Profile" }
    )]
    [ProducesResponseType(typeof(PsychologistVerificationResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetVerificationStatus()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(PsychologistResourceAssembler.ToErrorResponse("Invalid user session"));
            }

            _logger.LogInformation("Get verification status for psychologist: {UserId}", userId);

            var psychologist = await _psychologistQueryService.HandleGetPsychologistByIdAsync(userId);
            if (psychologist == null)
            {
                return NotFound(PsychologistResourceAssembler.ToErrorResponse("Psychologist profile not found"));
            }

            var response = PsychologistResourceAssembler.ToVerificationResource(psychologist);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting verification status");
            return StatusCode(StatusCodes.Status500InternalServerError,
                PsychologistResourceAssembler.ToErrorResponse("An error occurred while retrieving verification status"));
        }
    }

    [HttpPut("verification")]
    [SwaggerOperation(
        Summary = "Update psychologist verification documents",
        Description = "Updates verification documents (license, diploma, ID, certificates) for psychologist verification. Use multipart/form-data.",
        OperationId = "UpdateVerification",
        Tags = new[] { "Psychologist Profile" }
    )]
    [ProducesResponseType(typeof(PsychologistVerificationResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateVerification([FromForm] PsychologistVerificationResource resource)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(PsychologistResourceAssembler.ToErrorResponse("Invalid user session"));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(PsychologistResourceAssembler.ToErrorResponse("Invalid request data"));
            }

            _logger.LogInformation("Update verification for psychologist: {UserId}", userId);

            // Upload files to Cloudinary and get URLs
            string? licenseUrl = resource.LicenseDocumentUrl;
            string? diplomaUrl = resource.DiplomaCertificateUrl;
            string? identityUrl = resource.IdentityDocumentUrl;
            List<string>? additionalUrls = resource.AdditionalCertificatesUrls;

            // Upload license document if provided
            if (resource.LicenseDocumentFile != null && resource.LicenseDocumentFile.Length > 0)
            {
                using var licenseStream = new MemoryStream();
                await resource.LicenseDocumentFile.CopyToAsync(licenseStream);
                var licenseBytes = licenseStream.ToArray();
                licenseUrl = await _cloudinaryImageService.UploadDocumentAsync(
                    licenseBytes,
                    resource.LicenseDocumentFile.FileName,
                    _cloudinarySettings.PsychologistDocumentsFolder);
                _logger.LogInformation("License document uploaded for psychologist: {UserId}", userId);
            }

            // Upload diploma certificate if provided
            if (resource.DiplomaCertificateFile != null && resource.DiplomaCertificateFile.Length > 0)
            {
                using var diplomaStream = new MemoryStream();
                await resource.DiplomaCertificateFile.CopyToAsync(diplomaStream);
                var diplomaBytes = diplomaStream.ToArray();
                diplomaUrl = await _cloudinaryImageService.UploadDocumentAsync(
                    diplomaBytes,
                    resource.DiplomaCertificateFile.FileName,
                    _cloudinarySettings.PsychologistDocumentsFolder);
                _logger.LogInformation("Diploma certificate uploaded for psychologist: {UserId}", userId);
            }

            // Upload identity document if provided
            if (resource.IdentityDocumentFile != null && resource.IdentityDocumentFile.Length > 0)
            {
                using var identityStream = new MemoryStream();
                await resource.IdentityDocumentFile.CopyToAsync(identityStream);
                var identityBytes = identityStream.ToArray();
                identityUrl = await _cloudinaryImageService.UploadDocumentAsync(
                    identityBytes,
                    resource.IdentityDocumentFile.FileName,
                    _cloudinarySettings.PsychologistDocumentsFolder);
                _logger.LogInformation("Identity document uploaded for psychologist: {UserId}", userId);
            }

            // Upload additional certificates if provided
            if (resource.AdditionalCertificatesFiles != null && resource.AdditionalCertificatesFiles.Count > 0)
            {
                additionalUrls = new List<string>();
                foreach (var file in resource.AdditionalCertificatesFiles)
                {
                    if (file.Length > 0)
                    {
                        using var fileStream = new MemoryStream();
                        await file.CopyToAsync(fileStream);
                        var fileBytes = fileStream.ToArray();
                        var url = await _cloudinaryImageService.UploadDocumentAsync(
                            fileBytes,
                            file.FileName,
                            _cloudinarySettings.PsychologistDocumentsFolder);
                        additionalUrls.Add(url);
                    }
                }
                _logger.LogInformation("Additional certificates uploaded for psychologist: {UserId}, Count: {Count}", userId, additionalUrls.Count);
            }

            var command = PsychologistResourceAssembler.ToUpdateVerificationCommand(
                resource, userId, licenseUrl, diplomaUrl, identityUrl, additionalUrls);
            var updatedPsychologist = await _psychologistCommandService.HandleUpdateVerificationAsync(command);

            if (updatedPsychologist == null)
            {
                return BadRequest(PsychologistResourceAssembler.ToErrorResponse("Failed to update verification information"));
            }

            var response = PsychologistResourceAssembler.ToVerificationResource(updatedPsychologist);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating verification");
            return StatusCode(StatusCodes.Status500InternalServerError,
                PsychologistResourceAssembler.ToErrorResponse("An error occurred while updating verification"));
        }
    }

    [HttpGet("invitation-code")]
    [SwaggerOperation(
        Summary = "Get psychologist invitation code",
        Description = "Retrieves or auto-generates the invitation code for the verified psychologist. Patients use this code to connect.",
        OperationId = "GetInvitationCode",
        Tags = new[] { "Psychologist Profile" }
    )]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetInvitationCode()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(PsychologistResourceAssembler.ToErrorResponse("Invalid user session"));
            }

            _logger.LogInformation("Get invitation code for psychologist: {UserId}", userId);

            var psychologist = await _psychologistQueryService.HandleGetPsychologistByIdAsync(userId);
            if (psychologist == null)
            {
                return NotFound(PsychologistResourceAssembler.ToErrorResponse("Psychologist profile not found"));
            }

            if (!psychologist.IsVerified)
            {
                return BadRequest(PsychologistResourceAssembler.ToErrorResponse("Psychologist must be verified to access invitation code"));
            }

            // Auto-generate code if it doesn't exist or is expired
            if (string.IsNullOrWhiteSpace(psychologist.InvitationCode) || psychologist.IsInvitationCodeExpired())
            {
                _logger.LogInformation("Auto-generating invitation code for psychologist: {UserId}", userId);

                var regenerateCommand = new RegenerateInvitationCodeCommand(userId, "Auto-generated on first access", userId);
                var newCode = await _psychologistCommandService.HandleRegenerateInvitationCodeAsync(regenerateCommand);

                if (string.IsNullOrWhiteSpace(newCode))
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        PsychologistResourceAssembler.ToErrorResponse("Failed to generate invitation code"));
                }

                // Fetch updated psychologist
                psychologist = await _psychologistQueryService.HandleGetPsychologistByIdAsync(userId);
            }

            var response = new
            {
                invitationCode = psychologist.InvitationCode,
                generatedAt = psychologist.InvitationCodeGeneratedAt,
                expiresAt = psychologist.InvitationCodeExpiresAt,
                isExpired = psychologist.IsInvitationCodeExpired(),
                timeUntilExpiration = psychologist.InvitationCodeExpiresAt - DateTime.UtcNow
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting invitation code");
            return StatusCode(StatusCodes.Status500InternalServerError,
                PsychologistResourceAssembler.ToErrorResponse("An error occurred while retrieving invitation code"));
        }
    }

    [HttpPost("regenerate-code")]
    [SwaggerOperation(
        Summary = "Regenerate invitation code",
        Description = "Manually regenerates a new invitation code for the psychologist. Previous code becomes invalid.",
        OperationId = "RegenerateInvitationCode",
        Tags = new[] { "Psychologist Profile" }
    )]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RegenerateInvitationCode()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(PsychologistResourceAssembler.ToErrorResponse("Invalid user session"));
            }

            _logger.LogInformation("Regenerate invitation code for psychologist: {UserId}", userId);

            var command = new RegenerateInvitationCodeCommand(userId, "Manual regeneration requested", userId);
            var newCode = await _psychologistCommandService.HandleRegenerateInvitationCodeAsync(command);

            if (string.IsNullOrWhiteSpace(newCode))
            {
                return BadRequest(PsychologistResourceAssembler.ToErrorResponse("Failed to regenerate invitation code"));
            }

            var response = new
            {
                invitationCode = newCode,
                generatedAt = DateTime.UtcNow,
                expiresAt = DateTime.UtcNow.AddDays(1),
                message = "Invitation code regenerated successfully"
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error regenerating invitation code");
            return StatusCode(StatusCodes.Status500InternalServerError,
                PsychologistResourceAssembler.ToErrorResponse("An error occurred while regenerating invitation code"));
        }
    }

    [HttpGet("complete")]
    [SwaggerOperation(
        Summary = "Get complete psychologist profile",
        Description = "Retrieves the complete profile including personal, professional, and verification information.",
        OperationId = "GetCompleteProfile",
        Tags = new[] { "Psychologist Profile" }
    )]
    [ProducesResponseType(typeof(PsychologistCompleteProfileResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCompleteProfile()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(PsychologistResourceAssembler.ToErrorResponse("Invalid user session"));
            }

            _logger.LogInformation("Get complete profile for psychologist: {UserId}", userId);

            var psychologist = await _psychologistQueryService.HandleGetPsychologistByIdAsync(userId);
            if (psychologist == null)
            {
                return NotFound(PsychologistResourceAssembler.ToErrorResponse("Psychologist profile not found"));
            }

            var response = PsychologistResourceAssembler.ToCompleteProfileResource(psychologist);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting complete profile");
            return StatusCode(StatusCodes.Status500InternalServerError,
                PsychologistResourceAssembler.ToErrorResponse("An error occurred while retrieving complete profile"));
        }
    }

    [HttpGet("professional")]
    [ProducesResponseType(typeof(PsychologistProfessionalResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProfessionalProfile()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(PsychologistResourceAssembler.ToErrorResponse("Invalid user session"));
            }

            _logger.LogInformation("Get professional profile for psychologist: {UserId}", userId);

            var psychologist = await _psychologistQueryService.HandleGetPsychologistByIdAsync(userId);
            if (psychologist == null)
            {
                return NotFound(PsychologistResourceAssembler.ToErrorResponse("Psychologist profile not found"));
            }

            var response = PsychologistResourceAssembler.ToProfessionalResource(psychologist);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting professional profile");
            return StatusCode(StatusCodes.Status500InternalServerError,
                PsychologistResourceAssembler.ToErrorResponse("An error occurred while retrieving professional profile"));
        }
    }

    [HttpPut("professional")]
    [ProducesResponseType(typeof(PsychologistProfessionalResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProfessionalProfile([FromBody] PsychologistProfessionalResource resource)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(PsychologistResourceAssembler.ToErrorResponse("Invalid user session"));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(PsychologistResourceAssembler.ToErrorResponse("Invalid request data"));
            }

            _logger.LogInformation("Update professional profile for psychologist: {UserId}", userId);

            var command = PsychologistResourceAssembler.ToUpdateProfessionalCommand(resource, userId);
            var updatedPsychologist = await _psychologistCommandService.HandleUpdateProfessionalProfileAsync(command);

            if (updatedPsychologist == null)
            {
                return BadRequest(PsychologistResourceAssembler.ToErrorResponse("Failed to update professional profile"));
            }

            var response = PsychologistResourceAssembler.ToProfessionalResource(updatedPsychologist);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating professional profile");
            return StatusCode(StatusCodes.Status500InternalServerError,
                PsychologistResourceAssembler.ToErrorResponse("An error occurred while updating professional profile"));
        }
    }

    [HttpGet("stats")]
    [ProducesResponseType(typeof(PsychologistStatsResource), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStats([FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(PsychologistResourceAssembler.ToErrorResponse("Invalid user session"));
            }

            _logger.LogInformation("Get stats for psychologist: {UserId}", userId);

            var query = new GetPsychologistStatsQuery(userId, fromDate, toDate);
            var stats = await _psychologistQueryService.HandleGetPsychologistStatsAsync(query);

            if (stats == null)
            {
                return NotFound(PsychologistResourceAssembler.ToErrorResponse("Psychologist stats not found"));
            }

            var response = PsychologistResourceAssembler.ToStatsResource(stats);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting psychologist stats");
            return StatusCode(StatusCodes.Status500InternalServerError,
                PsychologistResourceAssembler.ToErrorResponse("An error occurred while retrieving stats"));
        }
    }

    [HttpGet("patient/{id}")]
    [Authorize(Roles = "Psychologist")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPatientProfile(string id)
    {
        try
        {
            var psychologistId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(psychologistId))
            {
                return Unauthorized(PsychologistResourceAssembler.ToErrorResponse("Invalid user session"));
            }

            _logger.LogInformation("Psychologist {PsychologistId} requesting patient profile: {PatientId}", psychologistId, id);

            var patient = await _userFacade.GetUserByIdAsync(id);
            if (patient == null)
            {
                return NotFound(PsychologistResourceAssembler.ToErrorResponse("Patient not found"));
            }

            var response = UserResourceAssembler.ToProfileResource(patient);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patient profile: {PatientId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                PsychologistResourceAssembler.ToErrorResponse("An error occurred while retrieving patient profile"));
        }
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst("user_id")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}