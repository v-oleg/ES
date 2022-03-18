using System.Reflection;
using System.Runtime.CompilerServices;
using ES.Core.Attributes;

namespace ES.Core.Services.Abstractions;

public abstract class AggregateProjector<TProjection> : Projector
    where TProjection : Projection, new() 
{
    protected TProjection Value = new();

    private Dictionary<string, MethodInfo> EventHandlers
    {
        get
        {
            return GetType().GetMethods()
                .Where(m => m.GetCustomAttributes<AggregateEventAttribute>().Any())
                .ToDictionary(k => k.GetCustomAttribute<AggregateEventAttribute>()!.Event);
        }
    }

    public sealed override async Task InitAsync(Guid aggregateId)
    {
        Value.AggregateId = aggregateId;
        await FetchAsync();
    }

    public sealed override async Task HandleAsync<TEvent>(TEvent @event)
    {
        if (!EventHandlers.ContainsKey(@event.EventName))
        {
            await UpdateLasEventNumberAsync(@event.EventNumber);
            return;
        };

        var isAsync = EventHandlers[@event.EventName].GetCustomAttribute<AsyncStateMachineAttribute>() != null;
        if (isAsync)
        {
            await (Task) EventHandlers[@event.EventName].Invoke(this, new object[] {@event})!;

            return;
        }

        EventHandlers[@event.EventName].Invoke(this, new object?[] {@event});
        await SaveAsync();
        await UpdateLasEventNumberAsync(@event.EventNumber);
    }
    
    public sealed override async Task HandleAsync<TEvent>(IEnumerable<TEvent> events)
    {
        foreach (var @event in events)
        {
            if (!EventHandlers.ContainsKey(@event.EventName)) continue;
    
            var isAsync = EventHandlers[@event.EventName].GetCustomAttribute<AsyncStateMachineAttribute>() != null;
            if (isAsync)
            {
                await (Task) EventHandlers[@event.EventName].Invoke(this, new object[] {@event})!;
    
                return;
            }
    
            EventHandlers[@event.EventName].Invoke(this, new object?[] {@event});
        }
    }
}