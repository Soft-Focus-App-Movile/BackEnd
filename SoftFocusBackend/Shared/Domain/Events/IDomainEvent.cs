namespace SoftFocusBackend.Shared.Domain.Events;

/// <summary>
/// Interfaz base para todos los Domain Events
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Identificador único del evento
    /// </summary>
    string EventId { get; }
    
    /// <summary>
    /// Timestamp cuando ocurrió el evento
    /// </summary>
    DateTime OccurredOn { get; }
    
    /// <summary>
    /// Nombre del evento (para logging/debugging)
    /// </summary>
    string EventName { get; }
}