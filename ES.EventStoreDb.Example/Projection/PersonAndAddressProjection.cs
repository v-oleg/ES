using MongoDB.Bson.Serialization.Attributes;

namespace ES.EventStoreDb.Example.Projection;

public class PersonAndAddressProjection : Core.Services.Abstractions.Projection
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
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Address1 { get; set; } = null!;
    public string? Address2 { get; set; }
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public string ZipCode { get; set; } = null!;
    public string Country { get; set; } = null!;
}