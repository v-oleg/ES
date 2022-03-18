using ES.Core.Attributes;
using ES.Core.Events;
using ES.Core.Services.Abstractions;
using ES.EventStoreDb.Example.Aggregates;
using ES.EventStoreDb.Example.Projection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace ES.EventStoreDb.Example.Projectors;

[AggregateStream("merchant", nameof(People))]
public class PersonProjector : AggregateProjector<PersonProjection>
{
    private readonly IMongoCollection<BsonDocument> _peopleCollection;
    private readonly IMongoCollection<BsonDocument> _checkpointsCollection;

    public PersonProjector(MongoClient mongoClient)
    {
        var db = mongoClient.GetDatabase("ProjectionsDemo");
        _peopleCollection = db.GetCollection<BsonDocument>("People");
        _checkpointsCollection = db.GetCollection<BsonDocument>("Checkpoints");
    }

    [AggregateEvent("PersonCreated")]
    public void PersonCreated(AggregateEvent @event)
    {
    }

    [AggregateEvent("PersonNameUpdated")]
    public void PersonNameUpdated(AggregateEvent @event)
    {
        Value.FirstName = @event.Data["FirstName"].Value<string>();
        Value.LastName = @event.Data["LastName"].Value<string>();
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
            Value = BsonSerializer.Deserialize<PersonProjection>(personBson);
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