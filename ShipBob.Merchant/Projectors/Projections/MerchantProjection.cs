using ES.Core.Services.Abstractions;
using MongoDB.Bson.Serialization.Attributes;

namespace ShipBob.Merchant.Projectors.Projections;

public class MerchantProjection : Projection
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
    public string Website { get; set; } = null!;
    public Address Address { get; set; } = null!;
}

public class Address
{
    public string Address1 { get; set; } = null!;
    public string? Address2 { get; set; }
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public string ZipCode { get; set; } = null!;
    public string Country { get; set; } = null!;
}