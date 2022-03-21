using ES.Core.Attributes;
using ES.Core.Events;
using ES.Core.Services.Abstractions;
using ES.EventStoreDb.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using ShipBob.Merchant.Projectors.Projections;

namespace ShipBob.Merchant.Projectors;

[AllStreams]
public class MerchantWithOrdersProjector : AllStreamsProjector<MerchantWithOrdersProjection>
{
    private readonly IMongoCollection<BsonDocument> _merchantWithOrdersCollection;
    private readonly IMongoCollection<BsonDocument> _checkpointsCollection;
    
    public MerchantWithOrdersProjector(MongoClient mongoClient)
    {
        var db = mongoClient.GetDatabase("ProjectionsDemo");
        _merchantWithOrdersCollection = db.GetCollection<BsonDocument>("MerchantWithOrders");
        _checkpointsCollection = db.GetCollection<BsonDocument>("Checkpoints");
    }
    
    [Stream("merchant", "MerchantInformationUpdated")]
    public void MerchantInformationUpdated(Event e)
    {
        Value.Name = e.Data["Name"]!.Value<string>()!;
    }
    
    [Stream("order", "OrderFinancialInformationUpdated")]
    public void OrderFinancialInformationUpdated(Event e)
    {
        var orderId = e.AsAggregateEvent().AggregateId;
        var totalPrice = e.Data["TotalPrice"]!.Value<decimal>();
        var financialStatus = e.Data["FinancialStatus"]!.ToObject<FinancialStatus>();
        if (!Value.Orders.ContainsKey(orderId))
        {
            Value.Orders[orderId] = new Order
            {
                TotalPrice = totalPrice,
                FinancialStatus = financialStatus
            };
        }
        else
        {
            var order = Value.Orders[orderId];
            order.FinancialStatus = financialStatus;
            order.TotalPrice = totalPrice;
        }
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
        var personBson = await _merchantWithOrdersCollection.Find(new BsonDocument("_id", Value.Id)).FirstOrDefaultAsync();
        if (personBson != null)
        {
            Value = BsonSerializer.Deserialize<MerchantWithOrdersProjection>(personBson);
        }
    }

    protected override async Task SaveAsync()
    {
        await _merchantWithOrdersCollection.ReplaceOneAsync(new BsonDocument("_id", Value.Id), Value.ToBsonDocument(),
            new ReplaceOptions
            {
                IsUpsert = true
            });
    }
    
    public override async Task InitAsync(Event e)
    {
        var aggregateEvent = e.AsAggregateEvent();
        Value.AggregateId = aggregateEvent.AggregateType switch
        {
            "Order" => aggregateEvent.CorrelationId!.Value,
            _ => aggregateEvent.AggregateId
        };

        await FetchAsync();
    }
}