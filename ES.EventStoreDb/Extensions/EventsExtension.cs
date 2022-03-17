using System.Text;
using EventStore.Client;
using Newtonsoft.Json.Linq;
using ES.Core.Events;

namespace ES.EventStoreDb.Extensions;

public static class EventsExtension
{
    public static Event AsEvent(this EventRecord eventRecord) => new(GetEventJson(eventRecord));
    
    public static AggregateEvent AsAggregateEvent(this Event @event) => new(@event);

    public static AggregateEvent AsAggregateEvent(this EventRecord eventRecord) => new(GetEventJson(eventRecord));
    
    #region Helpers

    private static JObject GetEventJson(EventRecord eventRecord)
    {
        var eventDataAsString = Encoding.UTF8.GetString(eventRecord.Data.Span);
        var eventMetadataAsString = Encoding.UTF8.GetString(eventRecord.Metadata.Span);

        var @event = JObject.Parse(eventMetadataAsString);
        var eventData = JObject.Parse(eventDataAsString);
        @event["data"] = eventData;
        @event["eventNumber"] = eventRecord.EventNumber.ToString();

        return @event;
    }
    
    #endregion
}