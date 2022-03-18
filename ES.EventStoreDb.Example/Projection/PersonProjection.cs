using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;

namespace ES.EventStoreDb.Example.Projection;

public class PersonProjection : Core.Services.Abstractions.Projection
{
    private Guid _aggregateId;

    [BsonId] public string Id { get; private set; } 
    
    public sealed override Guid AggregateId
    {
        get => _aggregateId;
        set
        {
            _aggregateId = value;
            Id = _aggregateId.ToString().Replace("-", string.Empty);
        } 
    }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}