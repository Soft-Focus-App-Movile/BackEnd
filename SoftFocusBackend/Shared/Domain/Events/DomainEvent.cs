namespace SoftFocusBackend.Shared.Domain.Events;

/// <summary>
/// Clase base abstracta para Domain Events
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    public string EventId { get; }
    public DateTime OccurredOn { get; }
    public string EventName { get; }

    protected DomainEvent()
    {
        EventId = Guid.NewGuid().ToString();
        OccurredOn = DateTime.UtcNow;
        EventName = GetType().Name;
    }
}