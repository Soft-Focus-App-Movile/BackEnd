using SoftFocusBackend.Library.Domain.Model.ValueObjects;

namespace SoftFocusBackend.Library.Application.ACL.Services;

/// <summary>
/// Servicio ACL para integración con el bounded context Tracking (futuro)
/// Preparado para cuando se implemente el sistema de tracking emocional
/// </summary>
public interface ITrackingIntegrationService
{
    /// <summary>
    /// Obtiene la emoción actual del usuario desde el sistema de tracking
    /// </summary>
    Task<EmotionalTag?> GetCurrentEmotionAsync(string userId);

    /// <summary>
    /// Verifica si el bounded context Tracking está disponible
    /// </summary>
    Task<bool> IsTrackingContextAvailableAsync();

    /// <summary>
    /// Obtiene el historial de emociones recientes del usuario
    /// </summary>
    Task<List<EmotionHistory>> GetRecentEmotionHistoryAsync(string userId, int limit = 10);
}

/// <summary>
/// Historial de emociones (anti-corruption layer)
/// </summary>
public class EmotionHistory
{
    public EmotionalTag Emotion { get; set; }
    public DateTime RecordedAt { get; set; }
    public double Intensity { get; set; }
}
