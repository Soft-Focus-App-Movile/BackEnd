using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SoftFocusBackend.Library.Application.ACL.Services;
using SoftFocusBackend.Users.Application.Internal.OutboundServices;
using SoftFocusBackend.Users.Domain.Model.Aggregates;
using SoftFocusBackend.Users.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Library.Application.ACL.Implementations;

/// <summary>
/// Implementación del servicio ACL para integración con Users Context
/// </summary>
public class UserIntegrationService : IUserIntegrationService
{
    private readonly IUserFacade _userFacade;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UserIntegrationService> _logger;

    public UserIntegrationService(
        IUserFacade userFacade,
        IHttpContextAccessor httpContextAccessor,
        ILogger<UserIntegrationService> logger)
    {
        _userFacade = userFacade;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public Task<string> GetUserIdFromTokenAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext == null)
        {
            _logger.LogError("HttpContext is null");
            throw new UnauthorizedAccessException("Usuario no autenticado");
        }

        var userId = httpContext.User.FindFirst("user_id")?.Value ??
                     httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogError("User ID not found in token claims");
            throw new UnauthorizedAccessException("Usuario no autenticado");
        }

        return Task.FromResult(userId);
    }

    public async Task<UserType> GetUserTypeAsync(string userId)
    {
        var user = await _userFacade.GetUserByIdAsync(userId);

        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            throw new InvalidOperationException($"Usuario no encontrado: {userId}");
        }

        return user.UserType;
    }

    public async Task<bool> ValidateUserExistsAsync(string userId)
    {
        var user = await _userFacade.GetUserByIdAsync(userId);
        return user != null;
    }

    public async Task<bool> ValidatePatientBelongsToPsychologistAsync(string patientId, string psychologistId)
    {
        try
        {
            // Obtener el psicólogo
            var psychologist = await _userFacade.GetUserByIdAsync(psychologistId);

            if (psychologist == null || psychologist.UserType != UserType.Psychologist)
            {
                _logger.LogWarning("User is not a psychologist: {PsychologistId}", psychologistId);
                return false;
            }

            // Verificar que el paciente exista
            var patient = await _userFacade.GetUserByIdAsync(patientId);
            if (patient == null)
            {
                _logger.LogWarning("Patient not found: {PatientId}", patientId);
                return false;
            }

            // TODO: Implementar validación real cuando se tenga la relación paciente-psicólogo
            // Por ahora, si ambos usuarios existen y el psicólogo es válido, retornar true
            _logger.LogInformation(
                "Patient-Psychologist relationship validated (basic check): {PatientId} - {PsychologistId}",
                patientId, psychologistId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating patient-psychologist relationship");
            return false;
        }
    }

    public async Task<List<string>> GetPsychologistPatientsAsync(string psychologistId)
    {
        try
        {
            var psychologist = await _userFacade.GetUserByIdAsync(psychologistId);

            if (psychologist == null || psychologist.UserType != UserType.Psychologist)
            {
                _logger.LogWarning("User is not a psychologist: {PsychologistId}", psychologistId);
                return new List<string>();
            }

            // TODO: Implementar cuando se tenga la relación paciente-psicólogo en el modelo
            // Por ahora retornar lista vacía
            _logger.LogWarning(
                "GetPsychologistPatientsAsync not yet implemented, returning empty list for: {PsychologistId}",
                psychologistId);

            return new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting psychologist patients");
            return new List<string>();
        }
    }

    public async Task<UserBasicInfo> GetUserBasicInfoAsync(string userId)
    {
        var user = await _userFacade.GetUserByIdAsync(userId);

        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            throw new InvalidOperationException($"Usuario no encontrado: {userId}");
        }

        return new UserBasicInfo
        {
            Id = user.Id,
            Name = user.FullName,
            UserType = user.UserType,
            Email = user.Email
        };
    }
}
