using ES.Core.Commands;
using ES.Core.ConfigSettings;
using ES.Core.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;

namespace ShipBob.Merchant.Controllers;

[ApiController]
[Route("merchants")]
public class MerchantsController : ControllerBase
{
    private readonly ICommandHandler _commandHandler;
    private readonly IMongoCollection<BsonDocument> _peopleCollection;
    private readonly IEventReader _eventReader;
    private readonly ServiceOptions _serviceOptions;

    public MerchantsController(ICommandHandler commandHandler,
        IEventReader eventReader, IOptions<ServiceOptions> options)
    {
        _commandHandler = commandHandler;
        _eventReader = eventReader;
        _serviceOptions = options.Value;
    }

    [HttpPost]
    [Route("")]
    public async Task<IActionResult> AddMerchant([FromBody] Models.Merchant merchant)
    {
        await _commandHandler.HandleAsync(
            new Command("AddMerchant", nameof(Aggregates.Merchant), merchant.AggregateId, null, data: JObject.FromObject(merchant)));

        return Created($"/merchants/{merchant.AggregateId}", merchant.AggregateId);
    }
    
    [HttpPost]
    [Route("{aggregateId}/address")]
    public async Task<IActionResult> UpdateMerchantAddress([FromRoute] Guid aggregateId, [FromBody] Models.Merchant merchant)
    {
        await _commandHandler.HandleAsync(new Command("UpdateMerchantMailingAddress", nameof(Aggregates.Merchant), aggregateId,
            null, data: JObject.FromObject(merchant)));

        return Accepted();
    }
    
    [HttpPost]
    [Route("{aggregateId}")]
    public async Task<IActionResult> UpdateMerchantInformation([FromRoute] Guid aggregateId, [FromBody] Models.Merchant merchant)
    {
        await _commandHandler.HandleAsync(new Command("UpdateMerchantInformation", nameof(Aggregates.Merchant), aggregateId,
            null, data: JObject.FromObject(merchant)));

        return Accepted();
    }
    
    [HttpGet]
    [Route("")]
    public async Task<ActionResult<IEnumerable<Models.Merchant>>> GetMerchants()
    {
        return Ok();
    }
    
    [HttpGet]
    [Route("{aggregateId}")]
    public async Task<ActionResult<Models.Merchant>> GetMerchant([FromRoute] Guid aggregateId)
    {

        return Ok();
    }
}