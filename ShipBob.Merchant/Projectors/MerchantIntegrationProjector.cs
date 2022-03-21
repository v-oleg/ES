using ES.Core.Attributes;
using ES.Core.Events;
using ES.Core.Services.Abstractions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using ShipBob.Merchant.Models;
using ShipBob.Merchant.Projectors.Projections;
using MerchantIntegration = ShipBob.Merchant.Aggregates.MerchantIntegration;

namespace ShipBob.Merchant.Projectors;

[AggregateStream("merchant", nameof(MerchantIntegration))]
public class MerchantIntegrationProjector : AggregateProjector<MerchantIntegrationProjection>
{
    private readonly IMongoCollection<BsonDocument> _merchantIntegrationCollection;
    private readonly IMongoCollection<BsonDocument> _checkpointCollection;
    
    public MerchantIntegrationProjector(MongoClient mongoClient)
    {
        var db = mongoClient.GetDatabase("ProjectionsDemo");
        _merchantIntegrationCollection = db.GetCollection<BsonDocument>("MerchantUsers");
        _checkpointCollection = db.GetCollection<BsonDocument>("Checkpoints");
    }
    
    [AggregateEvent("MerchantIntegrationAdded")]
    public void MerchantUserAdded(AggregateEvent e)
    {
        var id = e.Data["Id"]!.Value<int>();
        Value.Integrations[id] = new Integration
        {
            Id = id,
            Platform = e.Data["Platform"]!.ToObject<Platform>()
        };
    }
    
    [AggregateEvent("MerchantIntegrationUpdated")]
    public void MerchantUserInformationUpdated(AggregateEvent e)
    {
        var id = e.Data["Id"]!.Value<int>();
        var integration = Value.Integrations[id];
        integration.StoreUrl = e.Data["StoreUrl"]!.Value<string>()!;
    }
    
    [AggregateEvent("MerchantIntegrationTokenUpdated")]
    public void MerchantIntegrationTokenUpdated(AggregateEvent e)
    {
        var id = e.Data["Id"]!.Value<int>();
        var integration = Value.Integrations[id];
        integration.Token = e.Data["Token"]!.ToObject<string>()!;
    }
    
    [AggregateEvent("MerchantIntegrationDeleted")]
    public void MerchantIntegrationDeleted(AggregateEvent e)
    {
        var id = e.Data["Id"]!.Value<int>();
        Value.Integrations.Remove(id);
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
        var merchantBson = await _merchantIntegrationCollection.Find(new BsonDocument("_id", Value.Id)).FirstOrDefaultAsync();
        if (merchantBson != null)
        {
            Value = BsonSerializer.Deserialize<MerchantIntegrationProjection>(merchantBson);
        }
    }

    protected override async Task SaveAsync()
    {
        await _merchantIntegrationCollection.ReplaceOneAsync(new BsonDocument("_id", Value.Id), Value.ToBsonDocument(),
            new ReplaceOptions
            {
                IsUpsert = true
            });
    }
}