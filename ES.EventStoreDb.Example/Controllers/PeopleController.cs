using ES.Core.Commands;
using ES.Core.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using ES.EventStoreDb.Example.Aggregates;
using ES.EventStoreDb.Example.RequestModels;
using ES.EventStoreDb.Example.ViewModels;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace ES.EventStoreDb.Example.Controllers;

[ApiController]
[Route("[controller]")]
public class PeopleController : ControllerBase
{
    private readonly ICommandHandler _commandHandler;
    private readonly IMongoCollection<BsonDocument> _peopleCollection;

    public PeopleController(ICommandHandler commandHandler, MongoClient mongoClient)
    {
        _commandHandler = commandHandler;
        var db = mongoClient.GetDatabase("ProjectionsDemo");
        _peopleCollection = db.GetCollection<BsonDocument>("People");
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Person person)
    {
        var aggregateId = person.Id ?? Guid.NewGuid();
        var createPerson = new Command("CreatePerson", nameof(People), aggregateId, null,
            data: JObject.FromObject(person));

        await _commandHandler.HandleAsync(createPerson);
        
        return Created($"/people/{aggregateId}", aggregateId);
    }
    
    [HttpPatch]
    [Route("{id}")]
    public async Task<IActionResult> UpdatePersonName([FromRoute] Guid id, [FromBody] Person person)
    {
        var aggregateId = id;
        var createPerson = new Command("UpdatePersonName", nameof(People), aggregateId, null,
            data: JObject.FromObject(person));

        await _commandHandler.HandleAsync(createPerson);

        return Accepted();
    }
    
    [HttpPatch]
    [Route("{id}/address")]
    public async Task<IActionResult> UpdatePersonMailingAddress([FromRoute] Guid id, [FromBody] Person person)
    {
        var aggregateId = id;
        var createPerson = new Command("UpdatePersonMailingAddress", nameof(People), aggregateId, null,
            data: JObject.FromObject(person));

        await _commandHandler.HandleAsync(createPerson);

        return Accepted();
    }
    
    [HttpGet]
    [Route("{id}")]
    public async Task<ActionResult<PersonModel>> Get([FromRoute] Guid id)
    {
        var projection = Builders<BsonDocument>.Projection.Exclude("_id").Exclude("AggregateId");
        var personBson = await _peopleCollection.Find(new BsonDocument("AggregateId", id.ToString()))
            .Project(projection).FirstOrDefaultAsync();
        if (personBson != null)
        {
            
            return Ok(BsonSerializer.Deserialize<PersonModel>(personBson));
        }

        return NotFound();
    }
    
    [HttpGet]
    [Route("{id}/eventsource")]
    public async Task<ActionResult<PersonModel>> GetPerson([FromRoute] Guid id)
    {
        //TODO get person using projector on demand and using persistent datastore
        await Task.CompletedTask;
        
        return Ok(new Person());
    }
    
    [HttpGet]
    [Route("")]
    public async Task<ActionResult<IEnumerable<PersonModel>>> GetAll()
    {
        var projection = Builders<BsonDocument>.Projection.Exclude("_id").Exclude("AggregateId");
        var people = await _peopleCollection.Find(new BsonDocument()).Project(projection).ToListAsync();
        if (people != null)
        {
            return Ok(people.Select(p => BsonSerializer.Deserialize<PersonModel>(p)));
        }
        
        return Ok(new Person());
    }
}