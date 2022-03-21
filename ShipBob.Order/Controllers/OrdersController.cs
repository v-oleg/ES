using ES.Core.Commands;
using ES.Core.ConfigSettings;
using ES.Core.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using ShipBob.Order.Models;

namespace ShipBob.Order.Controllers;

[ApiController]
[Route("orders")]
public class OrdersController : ControllerBase
{
    private readonly ICommandHandler _commandHandler;
    private readonly IMongoCollection<BsonDocument> _peopleCollection;
    private readonly IEventReader _eventReader;
    private readonly ServiceOptions _serviceOptions;

    public OrdersController(ICommandHandler commandHandler,
        IEventReader eventReader, IOptions<ServiceOptions> options)
    {
        _commandHandler = commandHandler;
        _eventReader = eventReader;
        _serviceOptions = options.Value;
    }

    [HttpPost]
    [Route("")]
    public async Task<IActionResult> IngestOrder([FromBody] Models.Order order)
    {
        await _commandHandler.HandleAsync(new Command("IngestOrder", nameof(Aggregates.Order), order.AggregateId, null,
            data: JObject.FromObject(order)));
        return Created($"orders/{order.AggregateId}", order.AggregateId);
    }
    
    [HttpPost]
    [Route("{aggregateId}/address")]
    public async Task<IActionResult> UpdateOrderShippingAddress([FromRoute] Guid aggregateId, [FromBody] Models.Order order)
    {
        await _commandHandler.HandleAsync(new Command("UpdateOrderShippingAddress", nameof(Aggregates.Order), aggregateId, null,
            data: JObject.FromObject(order)));
        return Accepted();
    }
    
    [HttpPost]
    [Route("{aggregateId}/financial")]
    public async Task<IActionResult> UpdateOrderFinancialInformation([FromRoute] Guid aggregateId, [FromBody] Models.Order order)
    {
        await _commandHandler.HandleAsync(new Command("UpdateOrderFinancialInformation", nameof(Aggregates.Order), aggregateId, null,
            data: JObject.FromObject(order)));
        return Accepted();
    }
    
    [HttpPost]
    [Route("{aggregateId}/additems")]
    public async Task<IActionResult> AddOrderItem([FromRoute] Guid aggregateId, [FromBody] OrderItem order)
    {
        await _commandHandler.HandleAsync(new Command("AddOrderItem", nameof(Aggregates.Order), aggregateId, null,
            data: JObject.FromObject(order)));
        return Accepted();
    }
    
    [HttpPost]
    [Route("{aggregateId}/deleteitems")]
    public async Task<IActionResult> DeleteOrderItem([FromRoute] Guid aggregateId, [FromBody] OrderItem order)
    {
        await _commandHandler.HandleAsync(new Command("DeleteOrderItem", nameof(Aggregates.Order), aggregateId, null,
            data: JObject.FromObject(order)));
        return Accepted();
    }

    [HttpGet]
    [Route("")]
    public async Task<ActionResult<IEnumerable<Models.Order>>> GetOrders()
    {
        return Ok();
    }
    
    [HttpGet]
    [Route("{aggregateId}")]
    public async Task<ActionResult<IEnumerable<Models.Order>>> GetOrder([FromRoute] Guid aggregateId)
    {
        return Ok();
    }
}