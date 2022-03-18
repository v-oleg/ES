using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json.Linq;

[assembly:InternalsVisibleTo("ES.Core.Test")]
namespace ES.Core.Events;

public class Event
{
    protected readonly JObject EventJson;

    public Event(JObject eventJson, string eventType = nameof(Event))
    {
        EventJson = eventJson;
        EventJson["eventType"] = eventType;
    }

    public Guid EventId => EventJson.GetValue("eventId")!.ToObject<Guid>();

    public ulong EventNumber => EventJson.GetValue("eventNumber")!.Value<ulong>();

    public JObject Data => EventJson.GetValue("data")!.ToObject<JObject>()!;

    public string EventName => EventJson.GetValue("eventName")!.Value<string>()!;

    public DateTime EventDate => EventJson.GetValue("eventDate")!.ToObject<DateTime>().ToUniversalTime();

    public int EventVersion => EventJson.GetValue("eventVersion")!.Value<int>();

    public Guid? CorrelationId => EventJson.GetValue("correlationId")!.ToObject<Guid?>();
    
    public Guid? CausationId => EventJson.GetValue("causationId")!.ToObject<Guid?>();

    public string Service => EventJson.GetValue("service")!.Value<string>()!;

    public string EventType => EventJson.GetValue("eventType")!.Value<string>()!;

    internal JObject GetEventJsonReference() => EventJson;

    internal JObject GetEventJson() => new(EventJson);

    internal byte[] GetMetadata()
    {
        var metaDataToSerialize = new JObject();
        var properties = EventJson.Properties()
            .Where(p => p.Name != "eventType" && p.Name != "eventNumber" && p.Name != "data")
            .OrderBy(attr => attr.Name);

        foreach (var property in properties)
        {
            metaDataToSerialize.Add(property);
        }

        return Encoding.UTF8.GetBytes(metaDataToSerialize.ToString());
    }

    internal byte[] GetData() =>
        Encoding.UTF8.GetBytes(EventJson["data"]!.ToString());
}