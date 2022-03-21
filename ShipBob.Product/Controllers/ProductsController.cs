using ES.Core.Commands;
using ES.Core.ConfigSettings;
using ES.Core.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace ShipBob.Product.Controllers;

[ApiController]
[Route("products")]
public class ProductsController : ControllerBase
{
    private readonly ICommandHandler _commandHandler;
    private readonly IMongoCollection<BsonDocument> _peopleCollection;
    private readonly IEventReader _eventReader;
    private readonly ServiceOptions _serviceOptions;

    public ProductsController(ICommandHandler commandHandler, IEventReader eventReader, IOptions<ServiceOptions> options)
    {
        _commandHandler = commandHandler;
        _eventReader = eventReader;
        _serviceOptions = options.Value;
    }

    [HttpPost]
    [Route("")]
    public async Task<IActionResult> AddProduct([FromBody] Models.Product product)
    {
        await _commandHandler.HandleAsync(new Command("AddProduct", nameof(Aggregates.Product), product.AggregateId,
            null, data: JObject.FromObject(product)));

        return Created($"products/{product.AggregateId}", product.AggregateId);
    }
    
    [HttpPost]
    [Route("{aggregateId}")]
    public async Task<IActionResult> UpdateProductInformation([FromRoute] Guid aggregateId, [FromBody] Models.Product product)
    {
        await _commandHandler.HandleAsync(new Command("UpdateProductInformation", nameof(Aggregates.Product), aggregateId,
            null, data: JObject.FromObject(product)));

        return Accepted();
    }
    
    [HttpPost]
    [Route("{aggregateId}/properties")]
    public async Task<IActionResult> UpdateProductProperties([FromRoute] Guid aggregateId, [FromBody] Models.Product product)
    {
        await _commandHandler.HandleAsync(new Command("UpdateProductProperties", nameof(Aggregates.Product), aggregateId,
            null, data: JObject.FromObject(product)));

        return Accepted();
    }
    
    [HttpPost]
    [Route("{aggregateId}/delete")]
    public async Task<IActionResult> DeleteProduct([FromRoute] Guid aggregateId)
    {
        await _commandHandler.HandleAsync(new Command("DeleteProduct", nameof(Aggregates.Product), aggregateId,
            null));

        return Accepted();
    }
    
    [HttpGet]
    [Route("")]
    public async Task<ActionResult<IEnumerable<Models.Product>>> GetProducts()
    {

        return Ok();
    }
    
    [HttpGet]
    [Route("{aggregateId}")]
    public async Task<ActionResult<Models.Product>> GetProduct([FromRoute] Guid aggregateId)
    {
        return Ok();
    }
}