using ES.Core.Commands;
using ES.Core.Events;
using ES.Core.Services.Abstractions;
using Newtonsoft.Json.Linq;

namespace ES.Core.Services;

public class AggregateEventCreator : IAggregateEventCreator
{
    private readonly IEventCreator _eventCreator;

    public AggregateEventCreator(IEventCreator eventCreator)
    {
        _eventCreator = eventCreator;
    }

    public AggregateEvent AggregateEvent(Command command, string eventName, long? eventNumber, Action<JObject> data,
        int eventVersion = 1, string? authorizedUserId = null)
    {
        var @event = _eventCreator.CreateEvent(eventName, eventNumber, data, command.CorrelationId, command.CausationId,
            eventVersion, authorizedUserId);

        @event["aggregateId"] = command.AggregateId;
        @event["aggregateType"] = command.AggregateType;
        @event["commandName"] = command.CommandName;
        @event["commandId"] = command.CommandId;

        return new AggregateEvent(@event);
    }

    public AggregateEvent AggregateEvent(Command command, string eventName, long? eventNumber, JObject? data = null,
        int eventVersion = 1, string? authorizedUserId = null)
    {
        var @event = _eventCreator.CreateEvent(eventName, eventNumber, data, command.CorrelationId, command.CausationId,
            eventVersion, authorizedUserId);

        @event["aggregateId"] = command.AggregateId;
        @event["aggregateType"] = command.AggregateType;
        @event["commandName"] = command.CommandName;
        @event["commandId"] = command.CommandId;
        
        return new AggregateEvent(@event);
    }
}