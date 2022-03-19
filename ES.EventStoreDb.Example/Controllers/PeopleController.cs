using ES.Core;
using ES.Core.Commands;
using ES.Core.ConfigSettings;
using ES.Core.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using ES.EventStoreDb.Example.Aggregates;
using ES.EventStoreDb.Example.Projection;
using ES.EventStoreDb.Example.Projectors;
using ES.EventStoreDb.Example.RequestModels;
using ES.EventStoreDb.Example.Sql;
using ES.EventStoreDb.Example.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
    private readonly IEventReader _eventReader;
    private readonly ServiceOptions _serviceOptions;
    private readonly SqlDbContext _dbContext;

    public PeopleController(ICommandHandler commandHandler, MongoClient mongoClient, IEventReader eventReader,
        IOptions<ServiceOptions> options, SqlDbContext dbContext)
    {
        _commandHandler = commandHandler;
        _eventReader = eventReader;
        _dbContext = dbContext;
        var db = mongoClient.GetDatabase("ProjectionsDemo");
        _peopleCollection = db.GetCollection<BsonDocument>("People");
        _serviceOptions = options.Value;
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
        var events =
            await _eventReader.GetAggregateEventsAsync(
                Tools.Instance.Converter.ToAggregateIdStream(_serviceOptions.Name, nameof(People), id));

        var personEventSourcedProjector = new PersonEvenetSourcedProjector
        {
            Value =
            {
                AggregateId = id
            }
        };
        var person = await personEventSourcedProjector.HandleAsync(events) as PersonProjection;

        return Ok(new PersonModel
        {
            FirstName = person!.FirstName,
            LastName = person!.LastName,
        });
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

    [HttpGet]
    [Route("{id}/address")]
    public async Task<ActionResult<PersonAddressProjection>> GetAddress([FromRoute] Guid id)
    {
        var address = await _dbContext.PersonMailingAddress.AsNoTracking().SingleOrDefaultAsync(x => x.AggregateId == id);
        if (address == null) return NotFound();

        return Ok(address);
    }
}