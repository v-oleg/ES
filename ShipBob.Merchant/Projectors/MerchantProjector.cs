using ES.Core.Attributes;
using ES.Core.Events;
using ES.Core.Services.Abstractions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using ShipBob.Merchant.Projectors.Projections;

namespace ShipBob.Merchant.Projectors;

[AggregateStream("merchant", nameof(Aggregates.Merchant))]
public class MerchantProjector : AggregateProjector<MerchantProjection>
{
    private readonly IMongoCollection<BsonDocument> _merchantCollection;
    private readonly IMongoCollection<BsonDocument> _checkpointCollection;
    
    public MerchantProjector(MongoClient mongoClient)
    {
        var db = mongoClient.GetDatabase("ProjectionsDemo");
        _merchantCollection = db.GetCollection<BsonDocument>("Merchants");
        _checkpointCollection = db.GetCollection<BsonDocument>("Checkpoints");
    }
    
    [AggregateEvent("MerchantInformationUpdated")]
    public void MerchantInformationUpdated(AggregateEvent e)
    {
        Value.Name = e.Data["Name"]!.Value<string>()!;
        Value.Website = e.Data["Website"]!.Value<string>()!;
    }
    
    [AggregateEvent("MerchantMailingAddressUpdated")]
    public void MerchantMailingAddressUpdated(AggregateEvent e)
    {
        Value.Address = e.Data["Address"]!.ToObject<Address>()!;
    }

    public override async Task<ulong?> GetLasEventNumberAsync()
    {
        var checkpointBson = await _checkpointCollection.Find(new BsonDocument("Projector", GetType().FullName))
            .FirstOrDefaultAsync();
        if (checkpointBson == null) return default;

        var lastEventNumber = checkpointBson.GetElement("LastEventNumber");
        return (ulong) lastEventNumber.Value.AsInt64;
    }

    protected override async Task UpdateLasEventNumberAsync(ulong eventNumber)
    {
        await _checkpointCollection.ReplaceOneAsync(new BsonDocument("Projector", GetType().FullName),
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
        var merchantBson = await _merchantCollection.Find(new BsonDocument("_id", Value.Id)).FirstOrDefaultAsync();
        if (merchantBson != null)
        {
            Value = BsonSerializer.Deserialize<MerchantProjection>(merchantBson);
        }
    }

    protected override async Task SaveAsync()
    {
        await _merchantCollection.ReplaceOneAsync(new BsonDocument("_id", Value.Id), Value.ToBsonDocument(),
            new ReplaceOptions
            {
                IsUpsert = true
            });
    }
}