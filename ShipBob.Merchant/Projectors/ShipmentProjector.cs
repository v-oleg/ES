using ES.Core.Attributes;
using ES.Core.Events;
using ES.Core.Services.Abstractions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using ShipBob.Merchant.Projectors.Projections;

namespace ShipBob.Merchant.Projectors;

[AggregateStream("shipment", "Shipment")]
public class ShipmentProjector : AggregateProjector<ShipmentProjection>
{
    private readonly IMongoCollection<BsonDocument> _shipmentCollection;
    private readonly IMongoCollection<BsonDocument> _checkpointCollection;
    
    public ShipmentProjector(MongoClient mongoClient)
    {
        var db = mongoClient.GetDatabase("ProjectionsDemo");
        _shipmentCollection = db.GetCollection<BsonDocument>("Shipments");
        _checkpointCollection = db.GetCollection<BsonDocument>("Checkpoints");
    }

    [AggregateEvent("ShipmentAdded")]
    public void ShipmentAdded(AggregateEvent e)
    {
        Value.OrderId = e.Data["OrderId"]!.ToObject<Guid>();
    }
    
    [AggregateEvent("ShipmentShippingAddressUpdated")]
    public void ShipmentShippingAddressUpdated(AggregateEvent e)
    {
        Value.ShippingAddress = e.Data["ShippingAddress"]!.ToObject<Address>()!;
    }
    
    [AggregateEvent("ShipmentItemsAdded")]
    public void ShipmentItemsAdded(AggregateEvent e)
    {
        var shipmentItems = e.Data["ShipmentItems"]!.ToObject<IEnumerable<ShipmentItem>>()!;
        Value.ShipmentItems = shipmentItems.ToDictionary(k => k.ReferenceId);
    }
    
    [AggregateEvent("OrderItemAdded")]
    public void OrderItemAdded(AggregateEvent e)
    {
        var refId = e.Data["ReferenceId"]!.Value<string>()!;
        var price = e.Data["Price"]!.Value<decimal>();
        var quantity = e.Data["Quantity"]!.Value<int>();
        if (!Value.ShipmentItems.ContainsKey(refId))
        {
            Value.ShipmentItems[refId] = new ShipmentItem
            {
                ReferenceId = refId,
                Price = price,
                Quantity = quantity
            };
        }
        else
        {
            var item = Value.ShipmentItems[refId];
            item.Price = price;
            item.Quantity += quantity;
        }
    }
    
    [AggregateEvent("OrderItemDeleted")]
    public void OrderItemDeleted(AggregateEvent e)
    {
        var refId = e.Data["ReferenceId"]!.Value<string>()!;
        var quantity = e.Data["Quantity"]!.Value<int>();
        if (Value.ShipmentItems.ContainsKey(refId))
        {
            var item = Value.ShipmentItems[refId];
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
                    "LastEventNumber", (long)eventNumber
                }
            }, new ReplaceOptions
            {
                IsUpsert = true
            });
    }

    protected override async Task FetchAsync()
    {
        var merchantBson = await _shipmentCollection.Find(new BsonDocument("_id", Value.Id)).FirstOrDefaultAsync();
        if (merchantBson != null)
        {
            Value = BsonSerializer.Deserialize<ShipmentProjection>(merchantBson);
        }
    }

    protected override async Task SaveAsync()
    {
        await _shipmentCollection.ReplaceOneAsync(new BsonDocument("_id", Value.Id), Value.ToBsonDocument(),
            new ReplaceOptions
            {
                IsUpsert = true
            });
    }
}