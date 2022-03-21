using ES.Core.Services.Abstractions;
using MongoDB.Bson.Serialization.Attributes;

namespace ShipBob.Merchant.Projectors.Projections;

public class OrderProjection : Projection
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

    public decimal TotalPrice { get; set; }
    public FinancialStatus FinancialStatus { get; set; }
    public Address ShippingAddress { get; set; } = new();
    public Dictionary<string, OrderItem> OrderItems { get; set; } = new();
}

public class OrderItem
{
    public string ReferenceId { get; set; } = null!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public enum FinancialStatus
{
    Pending,
    Authorized,
    Paid
}