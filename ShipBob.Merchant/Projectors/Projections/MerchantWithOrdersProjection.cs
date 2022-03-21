using ES.Core.Services.Abstractions;
using MongoDB.Bson.Serialization.Attributes;

namespace ShipBob.Merchant.Projectors.Projections;

public class MerchantWithOrdersProjection : Projection
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
    public string Name { get; set; } = null!;

    public Dictionary<Guid, Order> Orders { get; set; } = new();
}

public class Order
{
    public Guid OrderId { get; set; }
    public decimal TotalPrice { get; set; }
    public FinancialStatus FinancialStatus { get; set; }
}