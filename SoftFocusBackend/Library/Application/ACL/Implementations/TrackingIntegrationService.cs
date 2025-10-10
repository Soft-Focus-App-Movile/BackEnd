using Microsoft.Extensions.Logging;
using SoftFocusBackend.Library.Application.ACL.Services;
using SoftFocusBackend.Library.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Library.Application.ACL.Implementations;

/// <summary>
/// Implementación del servicio ACL para integración con Tracking Context (futuro)
/// Actualmente retorna valores por defecto hasta que se implemente Tracking
/// </summary>
public class TrackingIntegrationService : ITrackingIntegrationService
{
    private readonly ILogger<TrackingIntegrationService> _logger;

    public TrackingIntegrationService(ILogger<TrackingIntegrationService> logger)
    {
        _logger = logger;
    }

    public Task<EmotionalTag?> GetCurrentEmotionAsync(string userId)
    {
        // TODO: Implementar cuando el bounded context Tracking esté disponible
        // Por ahora retornar null para indicar que no hay datos de tracking
        _logger.LogDebug("Tracking context not yet implemented, returning null emotion for user: {UserId}", userId);
        return Task.FromResult<EmotionalTag?>(null);
    }

    public Task<bool> IsTrackingContextAvailableAsync()
    {
        // TODO: Actualizar a true cuando el bounded context Tracking esté implementado
        return Task.FromResult(false);
    }

    public Task<List<EmotionHistory>> GetRecentEmotionHistoryAsync(string userId, int limit = 10)
    {
        // TODO: Implementar cuando el bounded context Tracking esté disponible
        _logger.LogDebug(
            "Tracking context not yet implemented, returning empty history for user: {UserId}",
            userId);
        return Task.FromResult(new List<EmotionHistory>());
    }
}
