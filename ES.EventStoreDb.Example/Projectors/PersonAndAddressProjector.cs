using ES.Core.Attributes;
using ES.Core.Events;
using ES.Core.Services.Abstractions;
using ES.EventStoreDb.Example.Projection;
using ES.EventStoreDb.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace ES.EventStoreDb.Example.Projectors;

[AllStreams]
public class PersonAndAddressProjector : AllStreamsProjector<PersonAndAddressProjection>
{
    private readonly IMongoCollection<BsonDocument> _peopleCollection;
    private readonly IMongoCollection<BsonDocument> _checkpointsCollection;

    public PersonAndAddressProjector(MongoClient mongoClient)
    {
        var db = mongoClient.GetDatabase("ProjectionsDemo");
        _peopleCollection = db.GetCollection<BsonDocument>("PersonAndAddress");
        _checkpointsCollection = db.GetCollection<BsonDocument>("Checkpoints");
    }
    
    [Stream("merchant", "PersonCreated")]
    public void PersonCreated(Event e)
    {
        Value.AggregateId = e.AsAggregateEvent().AggregateId;
    }
    
    [Stream("merchant", "PersonNameUpdated")]
    public void PersonNameUpdated(Event e)
    {
        Value.FirstName = e.Data["FirstName"]!.Value<string>()!;
        Value.LastName = e.Data["LastName"]!.Value<string>()!;
    }
    
    [Stream("merchant", "PersonMailingAddressUpdated")]
    public void PersonMailingAddressUpdated(Event e)
    {
        Value.Address1 = e.Data["Address1"]!.Value<string>()!;
        Value.Address2 = e.Data["Address2"]!.Value<string>()!;
        Value.City = e.Data["City"]!.Value<string>()!;
        Value.State = e.Data["State"]!.Value<string>()!;
        Value.ZipCode = e.Data["ZipCode"]!.Value<string>()!;
        Value.Country = e.Data["Country"]!.Value<string>()!;
    }

    public override async Task<ulong?> GetLasEventNumberAsync()
    {
        var checkpointBson = await _checkpointsCollection.Find(new BsonDocument("Projector", GetType().FullName))
            .FirstOrDefaultAsync();
        if (checkpointBson == null) return default;

        var lastEventNumber = checkpointBson.GetElement("LastEventNumber");
        return (ulong) lastEventNumber.Value.AsInt64;
    }

    protected override async Task UpdateLasEventNumberAsync(ulong eventNumber)
    {
        await _checkpointsCollection.ReplaceOneAsync(new BsonDocument("Projector", GetType().FullName),
            new BsonDocument
            {
                {
                    "Projector", GetType().FullName
                },
                {
                    "LastEventNumber", (long)eventNumber
                }
            }, new ReplaceOptions
            {
                IsUpsert = true
            });
    }

    protected override async Task FetchAsync()
    {
        var personBson = await _peopleCollection.Find(new BsonDocument("_id", Value.Id)).FirstOrDefaultAsync();
        if (personBson != null)
        {
            Value = BsonSerializer.Deserialize<PersonAndAddressProjection>(personBson);
        }
    }

    protected override async Task SaveAsync()
    {
        await _peopleCollection.ReplaceOneAsync(new BsonDocument("_id", Value.Id), Value.ToBsonDocument(),
            new ReplaceOptions
            {
                IsUpsert = true
            });
    }
}