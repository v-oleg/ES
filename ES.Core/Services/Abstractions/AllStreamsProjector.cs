using System.Reflection;
using System.Runtime.CompilerServices;
using ES.Core.Attributes;

namespace ES.Core.Services.Abstractions;

public abstract class AllStreamsProjector<TProjection> : Projector 
    where TProjection : Projection, new()
{
    public TProjection Value = new();

    private Dictionary<string, MethodInfo> EventHandlers
    {
        get
        {
            return GetType().GetMethods()
                .Where(m => m.GetCustomAttributes<StreamAttribute>().Any())
                .ToDictionary(k =>
                {
                    var stream = k.GetCustomAttribute<StreamAttribute>()!;
                    return $"{stream.Service}.{stream.Event}";
                });
        }
    }
    
    public sealed override Task InitAsync()
    {
        throw new NotImplementedException();
    }

    public sealed override async Task InitAsync(Guid aggregateId)
    {
        Value.AggregateId = aggregateId;
            await FetchAsync();
    }

    public sealed override async Task HandleAsync<TEvent>(TEvent @event)
    {
        var stream = $"{@event.Service}.{@event.EventName}";
        if (!EventHandlers.ContainsKey(stream))
        {
            await UpdateLasEventNumberAsync(@event.EventNumber);
            return;
        }

        var isAsync = EventHandlers[stream].GetCustomAttribute<AsyncStateMachineAttribute>() != null;
        if (isAsync)
        {
            await (Task) EventHandlers[stream].Invoke(this, new object[] {@event})!;

            return;
        }

        EventHandlers[stream].Invoke(this, new object?[] {@event});
        await SaveAsync();
        await UpdateLasEventNumberAsync(@event.EventNumber);
    }

    public sealed override async Task<object> HandleAsync<TEvent>(IEnumerable<TEvent> events)
    {
        foreach (var @event in events)
        {
            var stream = $"{@event.Service}.{@event.EventName}";
            if (!EventHandlers.ContainsKey(stream)) continue;
    
            var isAsync = EventHandlers[stream].GetCustomAttribute<AsyncStateMachineAttribute>() != null;
            if (isAsync)
            {
                await (Task) EventHandlers[stream].Invoke(this, new object[] {@event})!;
            }
            else
            {
                EventHandlers[stream].Invoke(this, new object?[] { @event });
            }
        }

        return Value;
    }
}