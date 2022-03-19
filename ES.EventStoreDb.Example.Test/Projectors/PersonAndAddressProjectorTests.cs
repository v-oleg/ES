using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ES.Core;
using ES.Core.Events;
using ES.EventStoreDb.Example.Projection;
using ES.EventStoreDb.Example.Projectors;
using ES.EventStoreDb.Extensions;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace ES.EventStoreDb.Example.Test.Projectors;

public class PersonAndAddressProjectorTests
{
    private const string EventsPath = "ES.EventStoreDb.Example.Test.Projectors.Events.";

    private readonly Mock<MongoClient> _mongoClient = new();

    [Fact]
    public async Task Should_Build_PersonAndAddress_Projection()
    {
        var personAndAddressProjector = new PersonAndAddressProjector(_mongoClient.Object);

        var projection = (PersonAndAddressProjection) await personAndAddressProjector.HandleAsync(GetEvents("PersonCreated",
            "PersonNameUpdated", "PersonMailingAddressUpdated"));

        Assert.Equal("Oleg", projection.FirstName);
        Assert.Equal("V", projection.LastName);
        Assert.Equal("Address1", projection.Address1);
        Assert.Equal("Address2", projection.Address2);
        Assert.Equal("City", projection.City);
        Assert.Equal("State", projection.State);
        Assert.Equal("ZipCode", projection.ZipCode);
        Assert.Equal("Country", projection.Country);
    }

    #region Helpers

    private IEnumerable<AggregateEvent> GetEvents(params string[] events)
    {
        var assembly = GetType().Assembly;
        var aggregateEvents = new List<AggregateEvent>();
        if (events.Length == 0)
        {
            aggregateEvents.Add(
                Tools.Instance.Converter.ResourceToEvent(assembly, $"{EventsPath}PersonCreated.json")
                    .AsAggregateEvent());
            aggregateEvents.Add(Tools.Instance.Converter
                .ResourceToEvent(assembly, $"{EventsPath}PersonNameUpdated.json")
                .AsAggregateEvent());
            aggregateEvents.Add(Tools.Instance.Converter
                .ResourceToEvent(assembly, $"{EventsPath}PersonMailingAddressUpdated.json")
                .AsAggregateEvent());
            aggregateEvents.Add(Tools.Instance.Converter
                .ResourceToEvent(assembly, $"{EventsPath}PersonNameUpdated_1.json")
                .AsAggregateEvent());

            return aggregateEvents;
        }

        return events.Select(@event => Tools.Instance.Converter
            .ResourceToEvent(assembly, $"{EventsPath}{@event}.json")
            .AsAggregateEvent());
    }

    #endregion
}