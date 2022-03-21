using ES.Core.Services.Abstractions;
using MongoDB.Bson.Serialization.Attributes;

namespace ShipBob.Merchant.Projectors.Projections;

public class ShipmentProjection : Projection
{
    private Guid _aggregateId;
    
    [BsonId] public string Id { get; private set; } = null!;

    public sealed override Guid AggregateId
    {
        get => _aggregateId;
        set
        {
            _aggregateId = value;
            Id = _aggregateId.ToString().Replace("-", string.Empty);
        } 
    }
    
    public Guid OrderId { get; set; } = default!;
    public Dictionary<string, ShipmentItem> ShipmentItems { get; set; } = new();
    public Address ShippingAddress { get; set; } = default!;
}

public class ShipmentItem
{
    public string ReferenceId { get; set; } = null!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}