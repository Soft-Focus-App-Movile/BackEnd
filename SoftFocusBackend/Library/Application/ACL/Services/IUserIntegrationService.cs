using SoftFocusBackend.Users.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Library.Application.ACL.Services;

/// <summary>
/// Servicio ACL para integración con el bounded context Users
/// </summary>
public interface IUserIntegrationService
{
    /// <summary>
    /// Obtiene el ID del usuario autenticado desde el token JWT
    /// </summary>
    Task<string> GetUserIdFromTokenAsync();

    /// <summary>
    /// Obtiene el tipo de usuario (General, Psychologist, Admin)
    /// </summary>
    Task<UserType> GetUserTypeAsync(string userId);

    /// <summary>
    /// Verifica que un usuario exista
    /// </summary>
    Task<bool> ValidateUserExistsAsync(string userId);

    /// <summary>
    /// Verifica que un paciente pertenezca a un psicólogo específico
    /// </summary>
    Task<bool> ValidatePatientBelongsToPsychologistAsync(string patientId, string psychologistId);

    /// <summary>
    /// Obtiene la lista de pacientes de un psicólogo
    /// </summary>
    Task<List<string>> GetPsychologistPatientsAsync(string psychologistId);

    /// <summary>
    /// Obtiene información básica de un usuario
    /// </summary>
    Task<UserBasicInfo> GetUserBasicInfoAsync(string userId);
}

/// <summary>
/// Información básica de un usuario (anti-corruption layer)
/// </summary>
public class UserBasicInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public UserType UserType { get; set; }
    public string Email { get; set; } = string.Empty;
}
