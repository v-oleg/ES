using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using ES.Core.Events;
using Xunit;

namespace ES.Core.Test.Models;

public class AggregateEventTests
{
    public static IEnumerable<object?[]> EventData
    {
        get
        {
            yield return new object?[]{ Guid.NewGuid(), Guid.NewGuid(), 1 };
            yield return new object?[]{ null, null, null };
        }
    }
    
    [Theory]
    [MemberData(nameof(EventData))]
    public void Should_Create_Aggregate_Event_With_Provided_Json(Guid? correlationId, Guid? causationId, long? eventNumber)
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
            ["service"] = "test",
            ["aggregateId"] = Guid.NewGuid(),
            ["aggregateType"] = "Test",
            ["commandId"] = Guid.NewGuid(),
            ["commandName"] = "TestCommand"
        };

        var aggregateEvent = new AggregateEvent(eventJson);
        
        Assert.Equal("AggregateEvent", aggregateEvent.EventType);
        Assert.Equal(eventJson["eventId"]!.ToObject<Guid>(), aggregateEvent.EventId);
        Assert.Equal(correlationId, aggregateEvent.CorrelationId);
        Assert.Equal(causationId, aggregateEvent.CausationId);
        Assert.Equal(eventNumber, aggregateEvent.EventNumber);
        Assert.Equal(eventJson["eventDate"]!.Value<DateTime>(), aggregateEvent.EventDate);
        Assert.Equal(eventJson["eventName"]!.Value<string>(), aggregateEvent.EventName);
        Assert.Equal(eventJson["eventVersion"]!.Value<int>(), aggregateEvent.EventVersion);
        Assert.Equal(eventJson["service"]!.Value<string>(), aggregateEvent.Service);
        Assert.Equal(eventJson["aggregateId"]!.ToObject<Guid>(), aggregateEvent.AggregateId);
        Assert.Equal(eventJson["aggregateType"]!.Value<string>(), aggregateEvent.AggregateType);
        Assert.Equal(eventJson["commandId"]!.ToObject<Guid>(), aggregateEvent.CommandId);
        Assert.Equal(eventJson["commandName"]!.Value<string>(), aggregateEvent.CommandName);
        Assert.Equal(eventJson["data"]!.ToObject<JObject>()!, @aggregateEvent.Data);
        Assert.Equal(eventJson, aggregateEvent.GetEventJson());
        Assert.Equal(eventJson, aggregateEvent.GetEventJsonReference());
        Assert.IsType<byte[]>(aggregateEvent.GetData());
        Assert.IsType<byte[]>(aggregateEvent.GetMetadata());
    }
    
    [Theory]
    [MemberData(nameof(EventData))]
    public void Should_Create_Aggregate_Event_With_Provided_Event(Guid? correlationId, Guid? causationId, long? eventNumber)
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
            ["service"] = "test",
            ["aggregateId"] = Guid.NewGuid(),
            ["aggregateType"] = "Test",
            ["commandId"] = Guid.NewGuid(),
            ["commandName"] = "TestCommand"
        };

        var @event = new Event(eventJson);
        var aggregateEvent = new AggregateEvent(@event);
        
        Assert.Equal("AggregateEvent", aggregateEvent.EventType);
        Assert.Equal(eventJson["eventId"]!.ToObject<Guid>(), aggregateEvent.EventId);
        Assert.Equal(correlationId, aggregateEvent.CorrelationId);
        Assert.Equal(causationId, aggregateEvent.CausationId);
        Assert.Equal(eventNumber, aggregateEvent.EventNumber);
        Assert.Equal(eventJson["eventDate"]!.Value<DateTime>(), aggregateEvent.EventDate);
        Assert.Equal(eventJson["eventName"]!.Value<string>(), aggregateEvent.EventName);
        Assert.Equal(eventJson["eventVersion"]!.Value<int>(), aggregateEvent.EventVersion);
        Assert.Equal(eventJson["service"]!.Value<string>(), aggregateEvent.Service);
        Assert.Equal(eventJson["aggregateId"]!.ToObject<Guid>(), aggregateEvent.AggregateId);
        Assert.Equal(eventJson["aggregateType"]!.Value<string>(), aggregateEvent.AggregateType);
        Assert.Equal(eventJson["commandId"]!.ToObject<Guid>(), aggregateEvent.CommandId);
        Assert.Equal(eventJson["commandName"]!.Value<string>(), aggregateEvent.CommandName);
        Assert.Equal(eventJson["data"]!.ToObject<JObject>()!, @aggregateEvent.Data);
        Assert.Equal(eventJson, aggregateEvent.GetEventJson());
        Assert.Equal(eventJson, aggregateEvent.GetEventJsonReference());
        Assert.IsType<byte[]>(aggregateEvent.GetData());
        Assert.IsType<byte[]>(aggregateEvent.GetMetadata());
    }
}