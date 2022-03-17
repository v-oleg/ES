using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json.Linq;
using ES.Core.Commands;
using ES.Core.ConfigSettings;
using ES.Core.Events;
using ES.Core.Services;
using ES.Core.Services.Abstractions;
using Xunit;

namespace ES.Core.Test.Aggregates;

public class AggregateTests
{
    private readonly Mock<IEventReader> _eventReader = new();
    private readonly Mock<IEventWriter> _eventWriter = new();
    private readonly Mock<IAggregateFactory> _aggregateFactory = new();
    private readonly Mock<IOptions<ServiceOptions>> _serviceOptions = new();

    private string PathToResources = "ES.Core.Test.Aggregates.Events.";

    [Fact]
    public async Task Should_Create_Person_When_Sending_CreatePerson_Command()
    {
        var aggregateId = Guid.NewGuid();

        var createPerson = new Command("CreatePerson", nameof(People), aggregateId, null, data: new JObject
        {
            ["FirstName"] = "Vasia",
            ["LastName"] = "Pupkin",
            ["Address"] = new JObject
            {
                ["Address1"] = "Address1",
                ["Address2"] = "Address2",
                ["City"] = "City",
                ["State"] = "State",
                ["ZipCode"] = "ZipCode",
                ["Country"] = "Country"
            }
        }, authorizedUserId: "userId or login or email who triggered the command");

        _eventReader.Setup(x => x.GetAggregateEventsAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<AggregateEvent>());

        _serviceOptions.Setup(x => x.Value).Returns(new ServiceOptions
        {
            Name = "merchant"
        });
        
        var eventCreator = new EventCreator(_serviceOptions.Object);
        var aggregateEventCreator = new AggregateEventCreator(eventCreator);

        _aggregateFactory.Setup(x => x.Create("People")).Returns(new People(aggregateEventCreator));

        var commandHandler = new CommandHandler(_eventReader.Object, _eventWriter.Object, _aggregateFactory.Object,
            _serviceOptions.Object);

        var events = (await commandHandler.HandleAsync(createPerson)).ToList();
        
        Assert.Equal(3, events.Count);
        
        Assert.Equal("PersonCreated", events[0].EventName);
        Assert.Equal(nameof(People), events[0].AggregateType);
        Assert.Equal("CreatePerson", events[0].CommandName);
        Assert.Equal("merchant", events[0].Service);
        Assert.Equal(nameof(AggregateEvent), events[0].EventType);
        Assert.Equal(aggregateId, events[0].AggregateId);
        Assert.Equal(0, events[0].EventNumber);
        Assert.Null(events[0].CorrelationId);
        Assert.Null(events[0].CausationId);
        Assert.Equal(new JObject(), events[0].Data);
        
        Assert.Equal("PersonNameUpdated", events[1].EventName);
        Assert.Equal(nameof(People), events[1].AggregateType);
        Assert.Equal("CreatePerson", events[1].CommandName);
        Assert.Equal("merchant", events[0].Service);
        Assert.Equal(nameof(AggregateEvent), events[0].EventType);
        Assert.Equal(aggregateId, events[1].AggregateId);
        Assert.Equal(1, events[1].EventNumber);
        Assert.Null(events[1].CorrelationId);
        Assert.Null(events[1].CausationId);
        Assert.Equal(new JObject { ["FirstName"] = "Vasia", ["LastName"] = "Pupkin" }, events[1].Data);
        
        Assert.Equal("PersonMailingAddressUpdated", events[2].EventName);
        Assert.Equal(nameof(People), events[2].AggregateType);
        Assert.Equal("CreatePerson", events[2].CommandName);
        Assert.Equal("merchant", events[2].Service);
        Assert.Equal(nameof(AggregateEvent), events[2].EventType);
        Assert.Equal(aggregateId, events[2].AggregateId);
        Assert.Equal(2, events[2].EventNumber);
        Assert.Null(events[2].CorrelationId);
        Assert.Null(events[2].CausationId);
        Assert.Equal("Address1", events[2].Data["Address"]!["Address1"]!.Value<string>());
    }
    
    [Fact]
    public async Task Should_Not_Create_Person_If_Exists()
    {
        var aggregateId = Guid.NewGuid();

        var createPerson = new Command("CreatePerson", nameof(People), aggregateId, null, data: new JObject
        {
            ["FirstName"] = "Vasia",
            ["LastName"] = "Pupkin",
            ["Address"] = new JObject
            {
                ["Address1"] = "Address1",
                ["Address2"] = "Address2",
                ["City"] = "City",
                ["State"] = "State",
                ["ZipCode"] = "ZipCode",
                ["Country"] = "Country"
            }
        }, authorizedUserId: "userId or login or email who triggered the command")
        {
            CommandId = Guid.Parse("f01d3029-f099-4436-8e92-a0d290fa5a48")
        };

        _eventReader.Setup(x => x.GetAggregateEventsAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<AggregateEvent>
            {
                new (ES.Core.Tools.Instance.Converter.ResourceToEvent(GetType().Assembly, $"{PathToResources}PersonCreated.json")),
                new (ES.Core.Tools.Instance.Converter.ResourceToEvent(GetType().Assembly, $"{PathToResources}PersonNameUpdated.json")),
                new (ES.Core.Tools.Instance.Converter.ResourceToEvent(GetType().Assembly, $"{PathToResources}PersonMailingAddressUpdated.json"))
            });

        _serviceOptions.Setup(x => x.Value).Returns(new ServiceOptions
        {
            Name = "merchant"
        });
        
        var eventCreator = new EventCreator(_serviceOptions.Object);
        var aggregateEventCreator = new AggregateEventCreator(eventCreator);

        _aggregateFactory.Setup(x => x.Create("People")).Returns(new People(aggregateEventCreator));

        var commandHandler = new CommandHandler(_eventReader.Object, _eventWriter.Object, _aggregateFactory.Object,
            _serviceOptions.Object);

        var events = (await commandHandler.HandleAsync(createPerson)).ToList();
        
        Assert.Equal(3, events.Count);
        
        Assert.Equal("PersonCreated", events[0].EventName);
        Assert.Equal(nameof(People), events[0].AggregateType);
        Assert.Equal("CreatePerson", events[0].CommandName);
        Assert.Equal("merchant", events[0].Service);
        Assert.Equal(nameof(AggregateEvent), events[0].EventType);
        Assert.Equal(Guid.Parse("784d17f9-12fa-45b5-905d-bc4ec84accc0"), events[0].AggregateId);
        Assert.Equal(0, events[0].EventNumber);
        Assert.Null(events[0].CorrelationId);
        Assert.Null(events[0].CausationId);
        Assert.Equal(new JObject(), events[0].Data);
        
        Assert.Equal("PersonNameUpdated", events[1].EventName);
        Assert.Equal(nameof(People), events[1].AggregateType);
        Assert.Equal("CreatePerson", events[1].CommandName);
        Assert.Equal("merchant", events[0].Service);
        Assert.Equal(nameof(AggregateEvent), events[0].EventType);
        Assert.Equal(Guid.Parse("784d17f9-12fa-45b5-905d-bc4ec84accc0"), events[0].AggregateId);
        Assert.Equal(1, events[1].EventNumber);
        Assert.Null(events[1].CorrelationId);
        Assert.Null(events[1].CausationId);
        Assert.Equal(new JObject { ["FirstName"] = "Vasia", ["LastName"] = "Pupkin" }, events[1].Data);
        
        Assert.Equal("PersonMailingAddressUpdated", events[2].EventName);
        Assert.Equal(nameof(People), events[2].AggregateType);
        Assert.Equal("CreatePerson", events[2].CommandName);
        Assert.Equal("merchant", events[2].Service);
        Assert.Equal(nameof(AggregateEvent), events[2].EventType);
        Assert.Equal(Guid.Parse("784d17f9-12fa-45b5-905d-bc4ec84accc0"), events[0].AggregateId);
        Assert.Equal(2, events[2].EventNumber);
        Assert.Null(events[2].CorrelationId);
        Assert.Null(events[2].CausationId);
        Assert.Equal("Address1", events[2].Data["Address"]!["Address1"]!.Value<string>());
        
        _eventWriter.VerifyNoOtherCalls();
    }
    
    [Fact]
    public async Task Should_Not_Update_Person_Name_If_Person_Does_Not_Exist()
    {
        var aggregateId = Guid.NewGuid();

        var createPerson = new Command("UpdatePersonName", nameof(People), aggregateId, null, data: new JObject
        {
            ["FirstName"] = "Vasia",
            ["LastName"] = "Pupkin",
            ["Address"] = new JObject
            {
                ["Address1"] = "Address1",
                ["Address2"] = "Address2",
                ["City"] = "City",
                ["State"] = "State",
                ["ZipCode"] = "ZipCode",
                ["Country"] = "Country"
            }
        }, authorizedUserId: "userId or login or email who triggered the command");

        _eventReader.Setup(x => x.GetAggregateEventsAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<AggregateEvent>());

        _serviceOptions.Setup(x => x.Value).Returns(new ServiceOptions
        {
            Name = "merchant"
        });
        
        var eventCreator = new EventCreator(_serviceOptions.Object);
        var aggregateEventCreator = new AggregateEventCreator(eventCreator);

        _aggregateFactory.Setup(x => x.Create("People")).Returns(new People(aggregateEventCreator));

        var commandHandler = new CommandHandler(_eventReader.Object, _eventWriter.Object, _aggregateFactory.Object,
            _serviceOptions.Object);

        await Assert.ThrowsAsync<TargetInvocationException>(async () => await commandHandler.HandleAsync(createPerson));
    }
}