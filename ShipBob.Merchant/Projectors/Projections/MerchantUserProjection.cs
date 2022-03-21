using ES.Core.Services.Abstractions;
using MongoDB.Bson.Serialization.Attributes;

namespace ShipBob.Merchant.Projectors.Projections;

public class MerchantUserProjection : Projection
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

    public Dictionary<int, MerchantUser> Users { get; } = new();
}

public class MerchantUser
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public bool Owner { get; set; } = false;
}