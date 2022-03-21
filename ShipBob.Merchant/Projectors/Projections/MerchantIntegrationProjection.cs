using ES.Core.Services.Abstractions;
using MongoDB.Bson.Serialization.Attributes;
using ShipBob.Merchant.Models;

namespace ShipBob.Merchant.Projectors.Projections;

public class MerchantIntegrationProjection : Projection
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

    public Dictionary<int, Integration> Integrations { get; } = new();
}

public class Integration
{
    public int Id { get; set; }
    public Platform Platform { get; set; }
    public string StoreUrl { get; set; } = null!;
    public string Token { get; set; } = null!;
}