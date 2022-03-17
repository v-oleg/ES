using Newtonsoft.Json.Linq;

namespace ES.Core.Events;

public class AggregateEvent : Event
{
    public AggregateEvent(JObject eventJson) : base(eventJson, nameof(AggregateEvent)) { }

    public AggregateEvent(Event @event) : base(@event.GetEventJsonReference(), nameof(AggregateEvent)) { }
    
    public string AggregateType => EventJson.GetValue("aggregateType")!.Value<string>()!;

    public Guid AggregateId => EventJson.GetValue("aggregateId")!.ToObject<Guid>();

    public string CommandName => EventJson.GetValue("commandName")!.Value<string>()!;

    public Guid CommandId => EventJson.GetValue("commandId")!.ToObject<Guid>();
    
    public string GetStreamName() => $"{Service}.{AggregateType}-{AggregateId}";
}