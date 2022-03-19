using ES.Core.Events;

namespace ES.Core.Services.Abstractions;

public abstract class Projector
{
    public abstract Task InitAsync(Guid aggregateId);
    
    public abstract Task HandleAsync<TEvent>(TEvent @event) where TEvent : Event;
    
    public abstract Task<object> HandleAsync<TEvent>(IEnumerable<TEvent> events) where TEvent : Event;
    
    public abstract Task<ulong?> GetLasEventNumberAsync();
    
    protected abstract Task UpdateLasEventNumberAsync(ulong eventNumber);

    protected abstract Task FetchAsync();

    protected abstract Task SaveAsync();
}