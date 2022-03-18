using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using ES.Core.Events;
using Xunit;

namespace ES.Core.Test.Models;

public class EventTests
{
    public static IEnumerable<object?[]> EventData
    {
        get
        {
            yield return new object?[]{ Guid.NewGuid(), Guid.NewGuid(), 1 };
            yield return new object?[]{ null, null, 0 };
        }
    }
    
    [Theory]
    [MemberData(nameof(EventData))]
    public void Should_Create_Event_With_Provided_Json(Guid? correlationId, Guid? causationId, ulong eventNumber)
    {
        var eventJson = new JObject
        {
            ["eventId"] = Guid.NewGuid(),
            ["eventNumber"] = eventNumber,
            ["data"] = new JObject
            {
                ["test"] = "test"
            },
            ["eventName"] = "EventHappened",
            ["eventDate"] = DateTime.UtcNow,
            ["eventVersion"] = 1,
            ["correlationId"] = correlationId,
            ["causationId"] = causationId,
            ["service"] = "test"
        };
        
        var @event = new Event(eventJson);

        Assert.Equal("Event", @event.EventType);
        Assert.Equal(eventJson["eventId"]!.ToObject<Guid>(), @event.EventId);
        Assert.Equal(correlationId, @event.CorrelationId);
        Assert.Equal(causationId, @event.CausationId);
        Assert.Equal(eventNumber, @event.EventNumber);
        Assert.Equal(eventJson["eventDate"]!.Value<DateTime>(), @event.EventDate);
        Assert.Equal(eventJson["eventName"]!.Value<string>(), @event.EventName);
        Assert.Equal(eventJson["eventVersion"]!.Value<int>(), @event.EventVersion);
        Assert.Equal(eventJson["service"]!.Value<string>(), @event.Service);
        Assert.Equal(eventJson["data"]!.ToObject<JObject>()!, @event.Data);
        Assert.Equal(eventJson, @event.GetEventJson());
        Assert.Equal(eventJson, @event.GetEventJsonReference());
        Assert.IsType<byte[]>(@event.GetData());
        Assert.IsType<byte[]>(@event.GetMetadata());
    }
}