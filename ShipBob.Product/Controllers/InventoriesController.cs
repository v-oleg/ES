using ES.Core.Commands;
using ES.Core.ConfigSettings;
using ES.Core.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using ShipBob.Product.Models;

namespace ShipBob.Product.Controllers;

[ApiController]
[Route("products/{aggregateId}/inventories")]
public class InventoriesController : ControllerBase
{
    private readonly ICommandHandler _commandHandler;
    private readonly IMongoCollection<BsonDocument> _peopleCollection;
    private readonly IEventReader _eventReader;
    private readonly ServiceOptions _serviceOptions;

    public InventoriesController(ICommandHandler commandHandler,
        IEventReader eventReader, IOptions<ServiceOptions> options)
    {
        _commandHandler = commandHandler;
        _eventReader = eventReader;
        _serviceOptions = options.Value;
    }

    [HttpPost]
    [Route("")]
    public async Task<IActionResult> AddInventory([FromRoute] Guid aggregateId, [FromBody] Models.Inventory inventory)
    {
        await _commandHandler.HandleAsync(new Command("AddInventory", nameof(Inventory), aggregateId, null,
            data: JObject.FromObject(inventory)));

        return Created($"inventories/{inventory.AggregateId}", inventory.AggregateId);
    }
    
    [HttpPost]
    [Route("addquantity")]
    public async Task<IActionResult> AddInventoryQuantity([FromRoute] Guid aggregateId, [FromBody] Models.Inventory inventory)
    {
        await _commandHandler.HandleAsync(new Command("AddInventoryQuantity", nameof(Inventory), aggregateId, null,
            data: JObject.FromObject(inventory)));

        return Accepted();
    }
    
    [HttpPost]
    [Route("allocate")]
    public async Task<IActionResult> AllocateInventory([FromRoute] Guid aggregateId)
    {
        await _commandHandler.HandleAsync(new Command("AllocateInventory", nameof(Inventory), aggregateId, null));

        return Accepted();
    }
    
    [HttpPost]
    [Route("deallocate")]
    public async Task<IActionResult> DeallocateInventory([FromRoute] Guid aggregateId)
    {
        await _commandHandler.HandleAsync(new Command("DeallocateInventory", nameof(Inventory), aggregateId, null));

        return Accepted();
    }
    
    [HttpGet]
    [Route("")]
    public async Task<ActionResult<IEnumerable<Models.Inventory>>> GetInventories()
    {
        return Ok();
    }
    
    [HttpGet]
    [Route("{id}")]
    public async Task<ActionResult<IEnumerable<Models.Inventory>>> GetInventory([FromRoute] Guid id)
    {
        return Ok();
    }
}