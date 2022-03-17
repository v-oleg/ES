using ES.Core.Commands;
using ES.Core.Events;
using Newtonsoft.Json.Linq;

namespace ES.Core.Services.Abstractions;

public interface IAggregateEventCreator
{
    AggregateEvent AggregateEvent(Command command, string eventName, long? eventNumber, Action<JObject> data,
        int eventVersion = 1, string? authorizedUserId = null);

    AggregateEvent AggregateEvent(Command command, string eventName, long? eventNumber, JObject? data = null,
        int eventVersion = 1, string? authorizedUserId = null);
}