using ES.Core.Attributes;
using ES.Core.Events;
using ES.Core.Services.Abstractions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using ShipBob.Merchant.Projectors.Projections;

namespace ShipBob.Merchant.Projectors;

[AggregateStream("merchant", nameof(Aggregates.MerchantUser))]
public class MerchantUserProjector : AggregateProjector<MerchantUserProjection>
{
    private readonly IMongoCollection<BsonDocument> _merchantUserCollection;
    private readonly IMongoCollection<BsonDocument> _checkpointCollection;
    
    public MerchantUserProjector(MongoClient mongoClient)
    {
        var db = mongoClient.GetDatabase("ProjectionsDemo");
        _merchantUserCollection = db.GetCollection<BsonDocument>("MerchantUsers");
        _checkpointCollection = db.GetCollection<BsonDocument>("Checkpoints");
    }

    [AggregateEvent("MerchantUserAdded")]
    public void MerchantUserAdded(AggregateEvent e)
    {
        var userId = e.Data["Id"]!.Value<int>();
        Value.Users[userId] = new Projections.MerchantUser
        {
            UserId = userId
        };
    }
    
    [AggregateEvent("MerchantUserInformationUpdated")]
    public void MerchantUserInformationUpdated(AggregateEvent e)
    {
        var userId = e.Data["Id"]!.Value<int>();
        var user = Value.Users[userId];
        user.FirstName = e.Data["FirstName"]!.Value<string>()!;
        user.LastName = e.Data["LastName"]!.Value<string>()!;
    }
    
    [AggregateEvent("MerchantUserOwnerAssigned")]
    public void MerchantUserOwnerAssigned(AggregateEvent e)
    {
        var userId = e.Data["Id"]!.Value<int>();
        var user = Value.Users[userId];
        user.Owner = true;
    }
    
    [AggregateEvent("MerchantUserOwnerUnassigned")]
    public void MerchantUserOwnerUnassigned(AggregateEvent e)
    {
        var userId = e.Data["Id"]!.Value<int>();
        var user = Value.Users[userId];
        user.Owner = false;
    }
    
    [AggregateEvent("MerchantUserDeleted")]
    public void MerchantUserDeleted(AggregateEvent e)
    {
        var userId = e.Data["Id"]!.Value<int>();
        Value.Users.Remove(userId);
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
        var merchantBson = await _merchantUserCollection.Find(new BsonDocument("_id", Value.Id)).FirstOrDefaultAsync();
        if (merchantBson != null)
        {
            Value = BsonSerializer.Deserialize<MerchantUserProjection>(merchantBson);
        }
    }

    protected override async Task SaveAsync()
    {
        await _merchantUserCollection.ReplaceOneAsync(new BsonDocument("_id", Value.Id), Value.ToBsonDocument(),
            new ReplaceOptions
            {
                IsUpsert = true
            });
    }
}