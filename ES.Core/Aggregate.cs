using System.Reflection;
using System.Runtime.CompilerServices;
using ES.Core.Attributes;
using ES.Core.Commands;
using ES.Core.Events;
using ES.Core.Services.Abstractions;
using Newtonsoft.Json.Linq;

namespace ES.Core;

public abstract class Aggregate
{
    private readonly IAggregateEventCreator _aggregateEventCreator;
    
    protected Aggregate(IAggregateEventCreator aggregateEventCreator)
    {
        _aggregateEventCreator = aggregateEventCreator;
    }

    public Guid? AggregateId { get; private set; } = null;
    public long AggregateVersion { get; protected set; } = -1;

    protected internal List<AggregateEvent> EventsToWrite { get; } = new();

    private Dictionary<string, MethodInfo> EventHandlers
    {
        get
        {
            return GetType().GetMethods()
                .Where(m => m.GetCustomAttributes<AggregateEventHandlerAttribute>().Any())
                .ToDictionary(k => k.GetCustomAttribute<AggregateEventHandlerAttribute>()!.EventName);
        }
    }
    
    private Dictionary<string, MethodInfo> CommandHandlers
    {
        get
        {
            return GetType().GetMethods()
                .Where(m => m.GetCustomAttributes<AggregateCommandHandlerAttribute>().Any())
                .ToDictionary(k => k.GetCustomAttribute<AggregateCommandHandlerAttribute>()!.CommandName);
        }
    }

    public async Task<IEnumerable<AggregateEvent>> HandleCommand(Command command)
    {   
        var isAsync = CommandHandlers[command.CommandName].GetCustomAttribute<AsyncStateMachineAttribute>() != null;

        if (isAsync)
        {
            var commandHandlerTask = CommandHandlers[command.CommandName]
                .Invoke(this, new object[] {command}) as Task ?? Task.FromResult(Enumerable.Empty<AggregateEvent>());

            await commandHandlerTask;
            
            return Enumerable.Empty<AggregateEvent>();
        }

        CommandHandlers[command.CommandName].Invoke(this, new object[] {command});

        return EventsToWrite;
    }

    public void ApplyEvents(IEnumerable<AggregateEvent> events)
    {
        foreach (var @event in events)
        {
            AggregateVersion++;
            if (EventHandlers.ContainsKey(@event.EventName))
                EventHandlers[@event.EventName].Invoke(this, new object[] {@event});
        }
    }

    protected void AddEvent(Command command, string eventName, Action<JObject> data,
        int eventVersion = 1, string? authorizedUserId = null)
    {
        EventsToWrite.Add(_aggregateEventCreator.AggregateEvent(command, eventName, ++AggregateVersion, data,
            eventVersion, authorizedUserId));
    }
    
    protected void AddEvent(Command command, string eventName, JObject data,
        int eventVersion = 1, string? authorizedUserId = null)
    {
        EventsToWrite.Add(_aggregateEventCreator.AggregateEvent(command, eventName, ++AggregateVersion, data,
            eventVersion, authorizedUserId));
    }
}