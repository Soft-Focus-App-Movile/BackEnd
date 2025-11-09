using Microsoft.Extensions.Logging;
using SoftFocusBackend.AI.Application.ACL.Services;

namespace SoftFocusBackend.AI.Application.ACL.Implementations;

/// <summary>
/// Implementación del servicio ACL para integración con Therapy Context
/// NOTA: Mantiene implementación mock hasta que Therapy context tenga goals/exercises en el domain model
/// </summary>
public class TherapyIntegrationService : ITherapyIntegrationService
{
    private readonly ILogger<TherapyIntegrationService> _logger;

    public TherapyIntegrationService(ILogger<TherapyIntegrationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<List<string>> GetCurrentTherapyGoalsAsync(string userId)
    {
        // TODO: Implementar cuando Therapy context tenga el concepto de "goals" en su domain model
        // Actualmente Therapy solo tiene TherapyPlan y TherapyProgress, sin goals explícitos
        // Pasos para implementación futura:
        // 1. Agregar TherapyGoal entity al Therapy domain model
        // 2. Crear ITherapyFacade con método GetUserTherapyGoalsAsync(string userId)
        // 3. Inyectar ITherapyFacade en este servicio
        // 4. Mapear TherapyGoal entities a List<string> de goals descriptivos
        _logger.LogDebug("Therapy goals not implemented yet. Returning empty goals list for user {UserId}", userId);
        return Task.FromResult(new List<string>());
    }

    public Task<List<string>> GetAssignedExercisesAsync(string userId)
    {
        // TODO: Implementar cuando Therapy context tenga el concepto de "exercises" en su domain model
        // Actualmente Therapy solo tiene TherapyPlan y TherapyProgress, sin exercises asignados
        // Pasos para implementación futura:
        // 1. Agregar TherapyExercise entity al Therapy domain model
        // 2. Agregar relación entre TherapyPlan y TherapyExercise
        // 3. Crear método en ITherapyFacade: GetUserAssignedExercisesAsync(string userId)
        // 4. Inyectar ITherapyFacade en este servicio
        // 5. Mapear TherapyExercise entities a List<string> de ejercicios descriptivos
        _logger.LogDebug("Therapy exercises not implemented yet. Returning empty exercises list for user {UserId}", userId);
        return Task.FromResult(new List<string>());
    }
}
