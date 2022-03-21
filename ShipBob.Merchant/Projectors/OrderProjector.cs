using ES.Core.Attributes;
using ES.Core.Events;
using ES.Core.Services.Abstractions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using ShipBob.Merchant.Projectors.Projections;

namespace ShipBob.Merchant.Projectors;

public class OrderProjector : AggregateProjector<OrderProjection>
{
    private readonly IMongoCollection<BsonDocument> _orderCollection;
    private readonly IMongoCollection<BsonDocument> _checkpointCollection;

    public OrderProjector(MongoClient mongoClient)
    {
        var db = mongoClient.GetDatabase("ProjectionsDemo");
        _orderCollection = db.GetCollection<BsonDocument>("Orders");
        _checkpointCollection = db.GetCollection<BsonDocument>("Checkpoints");
    }

    [AggregateEvent("OrderFinancialInformationUpdated")]
    public void OrderFinancialInformationUpdated(AggregateEvent e)
    {
        Value.TotalPrice = e.Data["TotalPrice"]!.Value<decimal>();
        Value.FinancialStatus = e.Data["FinancialStatus"]!.ToObject<FinancialStatus>();
    }

    [AggregateEvent("OrderShippingAddressUpdated")]
    public void OrderShippingAddressUpdated(AggregateEvent e)
    {
        Value.ShippingAddress = e.Data["ShippingAddress"]!.ToObject<Address>()!;
    }

    [AggregateEvent("OrderItemsAdded")]
    public void OrderItemsAdded(AggregateEvent e)
    {
        var orderItems = e.Data["OrderItems"]!.ToObject<IEnumerable<OrderItem>>()!;
        Value.OrderItems = orderItems.ToDictionary(k => k.ReferenceId);
    }
    
    [AggregateEvent("OrderItemAdded")]
    public void OrderItemAdded(AggregateEvent e)
    {
        var refId = e.Data["ReferenceId"]!.Value<string>()!;
        var price = e.Data["Price"]!.Value<decimal>();
        var quantity = e.Data["Quantity"]!.Value<int>();
        if (!Value.OrderItems.ContainsKey(refId))
        {
            Value.OrderItems[refId] = new OrderItem
            {
                ReferenceId = refId,
                Price = price,
                Quantity = quantity
            };
        }
        else
        {
            var item = Value.OrderItems[refId];
            item.Price = price;
            item.Quantity += quantity;
        }
    }
    
    [AggregateEvent("OrderItemDeleted")]
    public void OrderItemDeleted(AggregateEvent e)
    {
        var refId = e.Data["ReferenceId"]!.Value<string>()!;
        var quantity = e.Data["Quantity"]!.Value<int>();
        if (Value.OrderItems.ContainsKey(refId))
        {
            var item = Value.OrderItems[refId];
            item.Quantity -= quantity;
        }
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
                    "LastEventNumber", (long) eventNumber
                }
            }, new ReplaceOptions
            {
                IsUpsert = true
            });
    }

    protected override async Task FetchAsync()
    {
        var merchantBson = await _orderCollection.Find(new BsonDocument("_id", Value.Id)).FirstOrDefaultAsync();
        if (merchantBson != null)
        {
            Value = BsonSerializer.Deserialize<OrderProjection>(merchantBson);
        }
    }

    protected override async Task SaveAsync()
    {
        await _orderCollection.ReplaceOneAsync(new BsonDocument("_id", Value.Id), Value.ToBsonDocument(),
            new ReplaceOptions
            {
                IsUpsert = true
            });
    }
}