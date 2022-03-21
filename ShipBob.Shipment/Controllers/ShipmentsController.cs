using ES.Core.Commands;
using ES.Core.ConfigSettings;
using ES.Core.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace ShipBob.Shipment.Controllers;

[ApiController]
[Route("shipments")]
public class ShipmentsController : BaseController
{
    private readonly ICommandHandler _commandHandler;
    private readonly IMongoCollection<BsonDocument> _peopleCollection;
    private readonly IEventReader _eventReader;
    private readonly ServiceOptions _serviceOptions;

    public ShipmentsController(ICommandHandler commandHandler,
        IEventReader eventReader, IOptions<ServiceOptions> options) : base(commandHandler)
    {
        _commandHandler = commandHandler;
        _eventReader = eventReader;
        _serviceOptions = options.Value;
    }

    [HttpPost]
    [Route("")]
    public async Task<IActionResult> AddShipment([FromBody] Models.Shipment shipment)
    {
        var addShipment = new Command("AddShipment", nameof(Aggregates.Shipment), shipment.AggregateId,
            null, data: JObject.FromObject(shipment));
        await _commandHandler.HandleAsync(addShipment);
        return Created($"shipments/{shipment.AggregateId}", shipment.AggregateId);
    }
    
    [HttpPost]
    [Route("{aggregateId}/shipment")]
    public async Task<IActionResult> UpdateShipmentShippingAddress([FromRoute] Guid aggregateId, [FromBody] Models.Shipment shipment)
    {
        await _commandHandler.HandleAsync(new Command("UpdateShipmentShippingAddress", nameof(Aggregates.Shipment), aggregateId,
            null, data: JObject.FromObject(shipment)));
        return Accepted();
    }
    
    [HttpPost]
    [Route("{aggregateId}/additem")]
    public async Task<IActionResult> AddShipmentOrderItem([FromRoute] Guid aggregateId, [FromBody] Models.ShipmentItem shipment)
    {
        await _commandHandler.HandleAsync(new Command("AddShipmentItem", nameof(Aggregates.Shipment), aggregateId,
            null, data: JObject.FromObject(shipment)));
        return Accepted();
    }
    
    [HttpPost]
    [Route("{aggregateId}/deleteitem")]
    public async Task<IActionResult> DeleteShipmentOrderItem([FromRoute] Guid aggregateId, [FromBody] Models.ShipmentItem shipment)
    {
        await _commandHandler.HandleAsync(new Command("DeleteShipmentItem", nameof(Aggregates.Shipment), aggregateId,
            null, data: JObject.FromObject(shipment)));
        return Accepted();
    }
    
    [HttpGet]
    [Route("")]
    public async Task<IActionResult> GetShipments()
    {
        return Ok();
    }
    
    [HttpGet]
    [Route("{aggregateId}")]
    public async Task<IActionResult> GetShipments([FromRoute] Guid aggregateId)
    {
        return Ok();
    }
}