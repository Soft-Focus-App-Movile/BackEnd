using System.Security.Claims;
using SoftFocusBackend.Auth.Domain.Model.Commands;
using SoftFocusBackend.Auth.Domain.Model.Queries;
using SoftFocusBackend.Auth.Domain.Services;
using SoftFocusBackend.Auth.Interfaces.REST.Resources;
using SoftFocusBackend.Auth.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftFocusBackend.Auth.Application.ACL.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace SoftFocusBackend.Auth.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthCommandService _authCommandService;
    private readonly IAuthQueryService _authQueryService;
    private readonly IUserContextService _userContextService;  // AGREGAR
    private readonly ILogger<AuthController> _logger;
    
    public AuthController(
        IAuthCommandService authCommandService,
        IAuthQueryService authQueryService,
        IUserContextService userContextService,  
        ILogger<AuthController> logger)
    {
        _authCommandService = authCommandService ?? throw new ArgumentNullException(nameof(authCommandService));
        _authQueryService = authQueryService ?? throw new ArgumentNullException(nameof(authQueryService));
        _userContextService = userContextService ?? throw new ArgumentNullException(nameof(userContextService));  // AGREGAR
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterResource resource)
    {
        try
        {
            _logger.LogInformation("Registration attempt for email: {Email}", resource.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid registration request for email: {Email}", resource.Email);
                return BadRequest(AuthResourceAssembler.ToErrorResponse("Invalid request data"));
            }

            var command = AuthResourceAssembler.ToCommand(resource, GetClientIpAddress(), GetClientUserAgent());

            if (!command.IsValid())
            {
                _logger.LogWarning("Invalid registration data for email: {Email}", resource.Email);
                return BadRequest(AuthResourceAssembler.ToErrorResponse("Invalid registration data"));
            }

            var user = await _userContextService.CreateUserAsync(
                command.Email, command.Password, command.FullName, command.UserType,
                command.ProfessionalLicense, command.Specialties);

            if (user == null)
            {
                _logger.LogWarning("Registration failed for email: {Email}", resource.Email);
                return BadRequest(AuthResourceAssembler.ToErrorResponse("Registration failed"));
            }

            _logger.LogInformation("User registered successfully: {UserId}", user.Id);

            return Created($"/api/v1/users/{user.Id}", new {
                message = "Registration successful",
                userId = user.Id,
                email = user.Email
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for email: {Email}", resource.Email);
            return StatusCode(StatusCodes.Status500InternalServerError,
                AuthResourceAssembler.ToErrorResponse("An error occurred during registration"));
        }
    }

    [HttpPost("register/general")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Register general user (patient)",
        Description = "Creates a new general user account (patient). This endpoint is for users seeking psychological support.",
        OperationId = "RegisterGeneralUser",
        Tags = new[] { "Authentication" }
    )]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterGeneralUser([FromBody] RegisterGeneralUserResource resource)
    {
        try
        {
            _logger.LogInformation("General user registration attempt for email: {Email}", resource.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid general user registration request for email: {Email}", resource.Email);
                return BadRequest(AuthResourceAssembler.ToErrorResponse("Invalid request data"));
            }

            var command = AuthResourceAssembler.ToCommand(resource, GetClientIpAddress(), GetClientUserAgent());

            if (!command.IsValid())
            {
                _logger.LogWarning("Invalid general user registration data for email: {Email}", resource.Email);
                return BadRequest(AuthResourceAssembler.ToErrorResponse("Invalid registration data"));
            }

            var userId = await _authCommandService.HandleRegisterGeneralUserAsync(command);

            if (userId == null)
            {
                _logger.LogWarning("General user registration failed for email: {Email}", resource.Email);
                return BadRequest(AuthResourceAssembler.ToErrorResponse("Registration failed. Email may already be in use."));
            }

            _logger.LogInformation("General user registered successfully: {UserId}", userId);

            return Created($"/api/v1/users/{userId}", new
            {
                message = "General user registered successfully",
                userId = userId,
                email = resource.Email
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during general user registration for email: {Email}", resource.Email);
            return StatusCode(StatusCodes.Status500InternalServerError,
                AuthResourceAssembler.ToErrorResponse("An error occurred during registration"));
        }
    }

    [HttpPost("register/psychologist")]
    [AllowAnonymous]
    [Consumes("multipart/form-data")]
    [SwaggerOperation(
        Summary = "Register psychologist",
        Description = "Creates a new psychologist account. Requires professional documentation (license, diploma, DNI). Account will be pending verification by admin.",
        OperationId = "RegisterPsychologist",
        Tags = new[] { "Authentication" }
    )]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterPsychologist(
        [FromForm] string firstName,
        [FromForm] string lastName,
        [FromForm] string email,
        [FromForm] string password,
        [FromForm] string professionalLicense,
        [FromForm] int yearsOfExperience,
        [FromForm] string collegiateRegion,
        [FromForm] string university,
        [FromForm] int graduationYear,
        [FromForm] bool acceptsPrivacyPolicy,
        IFormFile licenseDocument,
        IFormFile diplomaDocument,
        IFormFile dniDocument,
        [FromForm] string? specialties = null, // comma-separated, optional
        List<IFormFile>? certificationDocuments = null) // optional
    {
        try
        {
            _logger.LogInformation("Psychologist registration attempt for email: {Email}", email);

            // STEP 1: Validate ModelState
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid psychologist registration request for email: {Email}", email);
                return BadRequest(AuthResourceAssembler.ToErrorResponse("Invalid request data"));
            }

            // STEP 2: Validate all required documents are provided
            if (licenseDocument == null)
            {
                return BadRequest(AuthResourceAssembler.ToErrorResponse("License document is required"));
            }
            if (diplomaDocument == null)
            {
                return BadRequest(AuthResourceAssembler.ToErrorResponse("Diploma document is required"));
            }
            if (dniDocument == null)
            {
                return BadRequest(AuthResourceAssembler.ToErrorResponse("DNI document is required"));
            }

            // STEP 3: Parse specialties (optional)
            string[]? specialtiesArray = null;
            if (!string.IsNullOrWhiteSpace(specialties))
            {
                specialtiesArray = specialties.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                              .Select(s => s.Trim())
                                              .ToArray();
            }

            // STEP 4: Validate command data BEFORE uploading files
            var tempCommand = new RegisterPsychologistCommand(
                firstName,
                lastName,
                email,
                password,
                professionalLicense,
                yearsOfExperience,
                collegiateRegion,
                specialtiesArray,
                university,
                graduationYear,
                acceptsPrivacyPolicy,
                null, // No document URLs yet
                null,
                null,
                null,
                GetClientIpAddress(),
                GetClientUserAgent());

            if (!tempCommand.IsValid())
            {
                _logger.LogWarning("Invalid psychologist registration data for email: {Email}", email);
                return BadRequest(AuthResourceAssembler.ToErrorResponse("Invalid registration data. Please check all fields."));
            }

            // STEP 5: Check if email already exists BEFORE uploading files
            var userContextService = HttpContext.RequestServices.GetRequiredService<SoftFocusBackend.Auth.Application.ACL.Services.IUserContextService>();
            var existingUser = await userContextService.GetUserByEmailAsync(email);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration failed - email already in use: {Email}", email);
                return BadRequest(AuthResourceAssembler.ToErrorResponse("Registration failed. Email already in use."));
            }

            // STEP 6: Now upload documents to Cloudinary (only after validation passes)
            var cloudinaryService = HttpContext.RequestServices.GetRequiredService<SoftFocusBackend.Shared.Infrastructure.ExternalServices.Cloudinary.Services.ICloudinaryImageService>();

            string licenseDocumentUrl;
            string diplomaDocumentUrl;
            string dniDocumentUrl;
            List<string> certificationDocumentUrls = new List<string>();

            // Upload license document
            {
                using var licenseStream = new MemoryStream();
                await licenseDocument.CopyToAsync(licenseStream);
                var licenseBytes = licenseStream.ToArray();

                var validation = cloudinaryService.ValidateDocument(licenseBytes, licenseDocument.FileName);
                if (!validation.IsValid)
                {
                    return BadRequest(AuthResourceAssembler.ToErrorResponse($"License document: {validation.ErrorMessage}"));
                }

                licenseDocumentUrl = await cloudinaryService.UploadDocumentAsync(licenseBytes, licenseDocument.FileName, "softfocus/psychologist_documents/licenses");
            }

            // Upload diploma document
            {
                using var diplomaStream = new MemoryStream();
                await diplomaDocument.CopyToAsync(diplomaStream);
                var diplomaBytes = diplomaStream.ToArray();

                var validation = cloudinaryService.ValidateDocument(diplomaBytes, diplomaDocument.FileName);
                if (!validation.IsValid)
                {
                    return BadRequest(AuthResourceAssembler.ToErrorResponse($"Diploma document: {validation.ErrorMessage}"));
                }

                diplomaDocumentUrl = await cloudinaryService.UploadDocumentAsync(diplomaBytes, diplomaDocument.FileName, "softfocus/psychologist_documents/diplomas");
            }

            // Upload DNI document
            {
                using var dniStream = new MemoryStream();
                await dniDocument.CopyToAsync(dniStream);
                var dniBytes = dniStream.ToArray();

                var validation = cloudinaryService.ValidateDocument(dniBytes, dniDocument.FileName);
                if (!validation.IsValid)
                {
                    return BadRequest(AuthResourceAssembler.ToErrorResponse($"DNI document: {validation.ErrorMessage}"));
                }

                dniDocumentUrl = await cloudinaryService.UploadDocumentAsync(dniBytes, dniDocument.FileName, "softfocus/psychologist_documents/dni");
            }

            // Upload certification documents (optional)
            if (certificationDocuments != null && certificationDocuments.Any())
            {
                foreach (var certDoc in certificationDocuments)
                {
                    using var certStream = new MemoryStream();
                    await certDoc.CopyToAsync(certStream);
                    var certBytes = certStream.ToArray();

                    var validation = cloudinaryService.ValidateDocument(certBytes, certDoc.FileName);
                    if (!validation.IsValid)
                    {
                        return BadRequest(AuthResourceAssembler.ToErrorResponse($"Certification document: {validation.ErrorMessage}"));
                    }

                    var certUrl = await cloudinaryService.UploadDocumentAsync(certBytes, certDoc.FileName, "softfocus/psychologist_documents/certifications");
                    certificationDocumentUrls.Add(certUrl);
                }
            }

            // STEP 7: Create final command with document URLs
            var command = new RegisterPsychologistCommand(
                firstName,
                lastName,
                email,
                password,
                professionalLicense,
                yearsOfExperience,
                collegiateRegion,
                specialtiesArray,
                university,
                graduationYear,
                acceptsPrivacyPolicy,
                licenseDocumentUrl,
                diplomaDocumentUrl,
                dniDocumentUrl,
                certificationDocumentUrls.Any() ? certificationDocumentUrls.ToArray() : null,
                GetClientIpAddress(),
                GetClientUserAgent());

            // STEP 8: Create the user
            var userId = await _authCommandService.HandleRegisterPsychologistAsync(command);

            if (userId == null)
            {
                _logger.LogWarning("Psychologist registration failed for email: {Email}", email);
                return BadRequest(AuthResourceAssembler.ToErrorResponse("Registration failed. An unexpected error occurred."));
            }

            _logger.LogInformation("Psychologist registered successfully: {UserId}", userId);

            return Created($"/api/v1/users/{userId}", new
            {
                message = "Psychologist registered successfully. Account pending verification.",
                userId = userId,
                email = email,
                documentsUploaded = new
                {
                    license = true,
                    diploma = true,
                    dni = true,
                    certifications = certificationDocumentUrls.Count
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during psychologist registration for email: {Email}", email);
            return StatusCode(StatusCodes.Status500InternalServerError,
                AuthResourceAssembler.ToErrorResponse("An error occurred during registration"));
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "User login",
        Description = "Authenticates a user with email and password. Returns JWT access token and user information upon successful authentication.",
        OperationId = "Login",
        Tags = new[] { "Authentication" }
    )]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] SignInResource resource)
    {
        try
        {
            _logger.LogInformation("Login attempt for email: {Email}", resource.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid login request for email: {Email}", resource.Email);
                return BadRequest(AuthResourceAssembler.ToErrorResponse("Invalid request data"));
            }

            var ipAddress = GetClientIpAddress();
            var userAgent = GetClientUserAgent();

            var command = AuthResourceAssembler.ToCommand(resource, ipAddress, userAgent);

            var authToken = await _authCommandService.HandleSignInAsync(command);

            if (authToken == null)
            {
                _logger.LogWarning("Login failed for email: {Email}", resource.Email);
                return Unauthorized(AuthResourceAssembler.ToErrorResponse("Invalid credentials"));
            }

            _logger.LogInformation("Login successful for user: {UserId}", authToken.GetUserId());

            var response = AuthResourceAssembler.ToSignInResponse(authToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            // Psychologist pending verification attempting login
            _logger.LogWarning(ex, "Login denied: {Message}", ex.Message);
            return Unauthorized(AuthResourceAssembler.ToErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", resource.Email);
            return StatusCode(StatusCodes.Status500InternalServerError,
                AuthResourceAssembler.ToErrorResponse("An error occurred during authentication"));
        }
    }

    [HttpPost("oauth")]
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> OAuthLogin([FromBody] OAuthSignInResource resource)
    {
        try
        {
            _logger.LogInformation("OAuth login attempt for provider: {Provider}", resource.Provider);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid OAuth login request for provider: {Provider}", resource.Provider);
                return BadRequest(AuthResourceAssembler.ToErrorResponse("Invalid request data"));
            }

            var ipAddress = GetClientIpAddress();
            var userAgent = GetClientUserAgent();

            var command = AuthResourceAssembler.ToCommand(resource, ipAddress, userAgent);

            var authToken = await _authCommandService.HandleOAuthSignInAsync(command);

            if (authToken == null)
            {
                _logger.LogWarning("OAuth login failed for provider: {Provider}", resource.Provider);
                return Unauthorized(AuthResourceAssembler.ToErrorResponse("OAuth authentication failed"));
            }

            _logger.LogInformation("OAuth login successful for user: {UserId}, provider: {Provider}",
                authToken.GetUserId(), resource.Provider);

            var response = AuthResourceAssembler.ToSignInResponse(authToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during OAuth login for provider: {Provider}", resource.Provider);
            return StatusCode(StatusCodes.Status500InternalServerError,
                AuthResourceAssembler.ToErrorResponse("An error occurred during OAuth authentication"));
        }
    }

    [HttpPost("oauth/verify")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Verify OAuth token",
        Description = "Verifies an OAuth token from Google/Facebook. Returns user information and whether additional registration is needed.",
        OperationId = "VerifyOAuth",
        Tags = new[] { "Authentication" }
    )]
    [ProducesResponseType(typeof(OAuthVerificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> VerifyOAuth([FromBody] OAuthVerifyResource resource)
    {
        try
        {
            _logger.LogInformation("OAuth verification attempt for provider: {Provider}", resource.Provider);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid OAuth verification request for provider: {Provider}", resource.Provider);
                return BadRequest(AuthResourceAssembler.ToErrorResponse("Invalid request data"));
            }

            var ipAddress = GetClientIpAddress();
            var userAgent = GetClientUserAgent();

            var command = AuthResourceAssembler.ToCommand(resource, ipAddress, userAgent);

            var result = await _authCommandService.HandleVerifyOAuthAsync(command);

            if (result == null)
            {
                _logger.LogWarning("OAuth verification failed for provider: {Provider}", resource.Provider);
                return Unauthorized(AuthResourceAssembler.ToErrorResponse("OAuth verification failed"));
            }

            _logger.LogInformation("OAuth verification successful for email: {Email}, needsRegistration: {NeedsRegistration}",
                result.Email, result.NeedsRegistration);

            var response = AuthResourceAssembler.ToResource(result);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            // Psychologist pending verification attempting OAuth login
            _logger.LogWarning(ex, "OAuth verification denied: {Message}", ex.Message);
            return Unauthorized(AuthResourceAssembler.ToErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during OAuth verification for provider: {Provider}", resource.Provider);
            return StatusCode(StatusCodes.Status500InternalServerError,
                AuthResourceAssembler.ToErrorResponse("An error occurred during OAuth verification"));
        }
    }

    [HttpPost("oauth/complete-registration")]
    [AllowAnonymous]
    [Consumes("multipart/form-data")]
    [SwaggerOperation(
        Summary = "Complete OAuth registration",
        Description = "Completes registration for OAuth users by providing additional required information (user type, professional documents for psychologists).",
        OperationId = "CompleteOAuthRegistration",
        Tags = new[] { "Authentication" }
    )]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CompleteOAuthRegistration(
        [FromForm] string tempToken,
        [FromForm] string userType,
        [FromForm] bool acceptsPrivacyPolicy,
        [FromForm] string? professionalLicense = null,
        [FromForm] int? yearsOfExperience = null,
        [FromForm] string? collegiateRegion = null,
        [FromForm] string? specialties = null, // comma-separated
        [FromForm] string? university = null,
        [FromForm] int? graduationYear = null,
        IFormFile? licenseDocument = null,
        IFormFile? diplomaDocument = null,
        IFormFile? dniDocument = null,
        List<IFormFile>? certificationDocuments = null)
    {
        try
        {
            _logger.LogInformation("OAuth registration completion attempt with temp token");

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid OAuth registration completion request");
                return BadRequest(AuthResourceAssembler.ToErrorResponse("Invalid request data"));
            }

            // Validate temp token first to get user info
            var tempTokenService = HttpContext.RequestServices.GetRequiredService<SoftFocusBackend.Auth.Infrastructure.OAuth.Services.IOAuthTempTokenService>();
            var oauthTempToken = await tempTokenService.ValidateAndRetrieveTokenAsync(tempToken);

            if (oauthTempToken == null)
            {
                _logger.LogWarning("Invalid or expired temp token for OAuth registration completion");
                return BadRequest(AuthResourceAssembler.ToErrorResponse("Invalid or expired registration token"));
            }

            string? licenseDocumentUrl = null;
            string? diplomaDocumentUrl = null;
            string? dniDocumentUrl = null;
            string[]? certificationDocumentUrls = null;

            // If user type is Psychologist, validate and upload documents
            if (userType == "Psychologist")
            {
                // Validate required fields for psychologist
                if (string.IsNullOrWhiteSpace(professionalLicense))
                {
                    return BadRequest(AuthResourceAssembler.ToErrorResponse("Professional license is required for psychologists"));
                }
                if (!yearsOfExperience.HasValue)
                {
                    return BadRequest(AuthResourceAssembler.ToErrorResponse("Years of experience is required for psychologists"));
                }
                if (string.IsNullOrWhiteSpace(collegiateRegion))
                {
                    return BadRequest(AuthResourceAssembler.ToErrorResponse("Collegiate region is required for psychologists"));
                }
                // Specialties are optional for psychologists
                if (string.IsNullOrWhiteSpace(university))
                {
                    return BadRequest(AuthResourceAssembler.ToErrorResponse("University is required for psychologists"));
                }
                if (!graduationYear.HasValue)
                {
                    return BadRequest(AuthResourceAssembler.ToErrorResponse("Graduation year is required for psychologists"));
                }

                // Validate required documents
                if (licenseDocument == null)
                {
                    return BadRequest(AuthResourceAssembler.ToErrorResponse("License document is required for psychologists"));
                }
                if (diplomaDocument == null)
                {
                    return BadRequest(AuthResourceAssembler.ToErrorResponse("Diploma document is required for psychologists"));
                }
                if (dniDocument == null)
                {
                    return BadRequest(AuthResourceAssembler.ToErrorResponse("DNI document is required for psychologists"));
                }
                // Certification documents are optional for psychologists

                // Upload documents to Cloudinary
                var cloudinaryService = HttpContext.RequestServices.GetRequiredService<SoftFocusBackend.Shared.Infrastructure.ExternalServices.Cloudinary.Services.ICloudinaryImageService>();

                // Upload license document
                {
                    using var licenseStream = new MemoryStream();
                    await licenseDocument.CopyToAsync(licenseStream);
                    var licenseBytes = licenseStream.ToArray();

                    var validation = cloudinaryService.ValidateDocument(licenseBytes, licenseDocument.FileName);
                    if (!validation.IsValid)
                    {
                        return BadRequest(AuthResourceAssembler.ToErrorResponse($"License document: {validation.ErrorMessage}"));
                    }

                    licenseDocumentUrl = await cloudinaryService.UploadDocumentAsync(licenseBytes, licenseDocument.FileName, "softfocus/psychologist_documents/licenses");
                }

                // Upload diploma document
                {
                    using var diplomaStream = new MemoryStream();
                    await diplomaDocument.CopyToAsync(diplomaStream);
                    var diplomaBytes = diplomaStream.ToArray();

                    var validation = cloudinaryService.ValidateDocument(diplomaBytes, diplomaDocument.FileName);
                    if (!validation.IsValid)
                    {
                        return BadRequest(AuthResourceAssembler.ToErrorResponse($"Diploma document: {validation.ErrorMessage}"));
                    }

                    diplomaDocumentUrl = await cloudinaryService.UploadDocumentAsync(diplomaBytes, diplomaDocument.FileName, "softfocus/psychologist_documents/diplomas");
                }

                // Upload DNI document
                {
                    using var dniStream = new MemoryStream();
                    await dniDocument.CopyToAsync(dniStream);
                    var dniBytes = dniStream.ToArray();

                    var validation = cloudinaryService.ValidateDocument(dniBytes, dniDocument.FileName);
                    if (!validation.IsValid)
                    {
                        return BadRequest(AuthResourceAssembler.ToErrorResponse($"DNI document: {validation.ErrorMessage}"));
                    }

                    dniDocumentUrl = await cloudinaryService.UploadDocumentAsync(dniBytes, dniDocument.FileName, "softfocus/psychologist_documents/dni");
                }

                // Upload certification documents (optional)
                if (certificationDocuments != null && certificationDocuments.Any())
                {
                    List<string> certUrls = new List<string>();
                    foreach (var certDoc in certificationDocuments)
                    {
                        using var certStream = new MemoryStream();
                        await certDoc.CopyToAsync(certStream);
                        var certBytes = certStream.ToArray();

                        var validation = cloudinaryService.ValidateDocument(certBytes, certDoc.FileName);
                        if (!validation.IsValid)
                        {
                            return BadRequest(AuthResourceAssembler.ToErrorResponse($"Certification document: {validation.ErrorMessage}"));
                        }

                        var certUrl = await cloudinaryService.UploadDocumentAsync(certBytes, certDoc.FileName, "softfocus/psychologist_documents/certifications");
                        certUrls.Add(certUrl);
                    }
                    certificationDocumentUrls = certUrls.ToArray();
                }
            }

            var ipAddress = GetClientIpAddress();
            var userAgent = GetClientUserAgent();

            // Parse specialties if provided
            string[]? specialtiesArray = null;
            if (!string.IsNullOrWhiteSpace(specialties))
            {
                specialtiesArray = specialties.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                              .Select(s => s.Trim())
                                              .ToArray();
            }

            var command = new CompleteOAuthRegistrationCommand(
                tempToken,
                oauthTempToken.Email,
                oauthTempToken.FullName,
                oauthTempToken.Provider,
                userType,
                acceptsPrivacyPolicy,
                professionalLicense,
                yearsOfExperience,
                collegiateRegion,
                specialtiesArray,
                university,
                graduationYear,
                licenseDocumentUrl,
                diplomaDocumentUrl,
                dniDocumentUrl,
                certificationDocumentUrls,
                ipAddress,
                userAgent);

            if (!command.IsValid())
            {
                _logger.LogWarning("Invalid OAuth registration completion data for email: {Email}", oauthTempToken.Email);
                return BadRequest(AuthResourceAssembler.ToErrorResponse("Invalid registration data"));
            }

            var authToken = await _authCommandService.HandleCompleteOAuthRegistrationAsync(command);

            if (authToken == null)
            {
                _logger.LogWarning("OAuth registration completion failed for email: {Email}", oauthTempToken.Email);
                return BadRequest(AuthResourceAssembler.ToErrorResponse("Registration failed"));
            }

            _logger.LogInformation("OAuth registration completed successfully for user: {UserId}", authToken.GetUserId());

            var response = AuthResourceAssembler.ToSignInResponse(authToken);
            return Created($"/api/v1/users/{authToken.GetUserId()}", response);
        }
        catch (UnauthorizedAccessException ex)
        {
            // Psychologist pending verification
            _logger.LogWarning(ex, "OAuth registration completed but access denied: {Message}", ex.Message);
            return Unauthorized(AuthResourceAssembler.ToErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during OAuth registration completion");
            return StatusCode(StatusCodes.Status500InternalServerError,
                AuthResourceAssembler.ToErrorResponse("An error occurred during registration"));
        }
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Request password reset",
        Description = "Sends a password reset email to the user if the email is registered. Always returns success for security reasons.",
        OperationId = "ForgotPassword",
        Tags = new[] { "Authentication" }
    )]
    [ProducesResponseType(typeof(PasswordResetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            _logger.LogInformation("Password reset request for email: {Email}", request.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid forgot password request for email: {Email}", request.Email);
                return BadRequest(AuthResourceAssembler.ToErrorResponse("Invalid request data"));
            }

            var ipAddress = GetClientIpAddress();
            var userAgent = GetClientUserAgent();

            var command = new SendPasswordResetCommand(request.Email, ipAddress, userAgent);

            var result = await _authCommandService.HandleSendPasswordResetAsync(command);

            _logger.LogInformation("Password reset request processed for email: {Email}", request.Email);

            var response = new PasswordResetResponse
            {
                Message = "Si el email está registrado, recibirás un enlace para restablecer tu contraseña.",
                Success = true
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing forgot password request for email: {Email}", request.Email);
            
            var response = new PasswordResetResponse
            {
                Message = "Si el email está registrado, recibirás un enlace para restablecer tu contraseña.",
                Success = true
            };
            
            return Ok(response);
        }
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Reset password",
        Description = "Resets user password using the token received via email. Token must be valid and not expired.",
        OperationId = "ResetPassword",
        Tags = new[] { "Authentication" }
    )]
    [ProducesResponseType(typeof(PasswordResetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            _logger.LogInformation("Password reset attempt for email: {Email}", request.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid reset password request for email: {Email}", request.Email);
                return BadRequest(AuthResourceAssembler.ToErrorResponse("Datos de solicitud inválidos"));
            }

            var ipAddress = GetClientIpAddress();
            var userAgent = GetClientUserAgent();

            var command = new ResetPasswordCommand(request.Token, request.Email, request.NewPassword, ipAddress, userAgent);

            var result = await _authCommandService.HandleResetPasswordAsync(command);

            if (!result)
            {
                _logger.LogWarning("Password reset failed for email: {Email}", request.Email);
                return BadRequest(AuthResourceAssembler.ToErrorResponse("Token inválido, expirado o datos incorrectos"));
            }

            _logger.LogInformation("Password reset successful for email: {Email}", request.Email);

            var response = new PasswordResetResponse
            {
                Message = "Contraseña restablecida exitosamente. Ahora puedes iniciar sesión con tu nueva contraseña.",
                Success = true
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset for email: {Email}", request.Email);
            return StatusCode(StatusCodes.Status500InternalServerError,
                AuthResourceAssembler.ToErrorResponse("Error interno del servidor"));
        }
    }

    #region Private Helper Methods

    private string? GetClientIpAddress()
    {
        try
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }
        catch
        {
            return null;
        }
    }

    private string? GetClientUserAgent()
    {
        try
        {
            return HttpContext.Request.Headers.UserAgent.FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    #endregion
}