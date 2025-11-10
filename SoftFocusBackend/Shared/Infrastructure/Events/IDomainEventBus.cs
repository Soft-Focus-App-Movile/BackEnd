using SoftFocusBackend.Shared.Domain.Events;

namespace SoftFocusBackend.Shared.Infrastructure.Events;

/// <summary>
/// Event Bus simple en memoria para publicar y suscribirse a Domain Events
/// </summary>
public interface IDomainEventBus
{
    Task PublishAsync<TEvent>(TEvent domainEvent) where TEvent : IDomainEvent;
    void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IDomainEvent;
}