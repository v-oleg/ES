using ES.Core.Commands;
using ES.Core.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using ES.EventStoreDb.Example.Aggregates;
using ES.EventStoreDb.Example.RequestModels;

namespace ES.EventStoreDb.Example.Controllers;

[ApiController]
[Route("[controller]")]
public class PeopleController : ControllerBase
{
    private readonly ICommandHandler _commandHandler;

    public PeopleController(ICommandHandler commandHandler)
    {
        _commandHandler = commandHandler;
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
    public async Task<ActionResult<Person>> Get([FromRoute] Guid id)
    {
        //TODO get person using projector on demand and using persistent datastore
        await Task.CompletedTask;
        
        return Ok(new Person());
    }
}