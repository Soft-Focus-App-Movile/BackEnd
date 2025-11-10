using SoftFocusBackend.Shared.Domain.Events;

namespace SoftFocusBackend.Shared.Infrastructure.Events;

public class DomainEventBus : IDomainEventBus
{
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();
    private readonly ILogger<DomainEventBus> _logger;
    private readonly IServiceProvider _serviceProvider;

    public DomainEventBus(ILogger<DomainEventBus> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : IDomainEvent
    {
        var eventType = typeof(TEvent);
        
        if (!_handlers.ContainsKey(eventType))
        {
            _handlers[eventType] = new List<Delegate>();
        }

        _handlers[eventType].Add(handler);
        
        _logger.LogInformation("Subscribed handler for event: {EventType}", eventType.Name);
    }

    public async Task PublishAsync<TEvent>(TEvent domainEvent) where TEvent : IDomainEvent
    {
        var eventType = typeof(TEvent);
        
        _logger.LogInformation(
            "Publishing event: {EventType} (ID: {EventId})", 
            domainEvent.EventName, 
            domainEvent.EventId);

        if (!_handlers.ContainsKey(eventType))
        {
            _logger.LogWarning("No handlers registered for event: {EventType}", eventType.Name);
            return;
        }

        var handlers = _handlers[eventType];
        var tasks = new List<Task>();

        foreach (var handler in handlers)
        {
            try
            {
                var typedHandler = (Func<TEvent, Task>)handler;
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await typedHandler(domainEvent);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, 
                            "Error in handler for event {EventType}: {Message}", 
                            eventType.Name, ex.Message);
                    }
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Error invoking handler for event {EventType}: {Message}", 
                    eventType.Name, ex.Message);
            }
        }

        // Ejecutar todos los handlers en paralelo
        await Task.WhenAll(tasks);
        
        _logger.LogInformation(
            "Event {EventType} processed by {Count} handlers", 
            eventType.Name, handlers.Count);
    }
}