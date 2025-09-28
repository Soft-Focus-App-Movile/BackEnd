using System.Security.Claims;
using SoftFocusBackend.Auth.Domain.Model.Commands;
using SoftFocusBackend.Auth.Domain.Model.Queries;
using SoftFocusBackend.Auth.Domain.Services;
using SoftFocusBackend.Auth.Interfaces.REST.Resources;
using SoftFocusBackend.Auth.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoftFocusBackend.Auth.Application.ACL.Services;

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

    [HttpPost("login")]
    [AllowAnonymous]
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", resource.Email);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                AuthResourceAssembler.ToErrorResponse("An error occurred during authentication"));
        }
    }

    [HttpPost("oauth")]
    [AllowAnonymous]
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

    [HttpPost("forgot-password")]
    [AllowAnonymous]
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