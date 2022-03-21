using ES.Core.Commands;
using ES.Core.ConfigSettings;
using ES.Core.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using ShipBob.Merchant.Models;

namespace ShipBob.Merchant.Controllers;

[ApiController]
[Route("merchants/{aggregateId}/integrations")]
public class MerchantIntegrationsController : ControllerBase
{
    private readonly ICommandHandler _commandHandler;
    private readonly IMongoCollection<BsonDocument> _peopleCollection;
    private readonly IEventReader _eventReader;
    private readonly ServiceOptions _serviceOptions;

    public MerchantIntegrationsController(ICommandHandler commandHandler,
        IEventReader eventReader, IOptions<ServiceOptions> options)
    {
        _commandHandler = commandHandler;
        _eventReader = eventReader;
        _serviceOptions = options.Value;
    }
    
    [HttpPost]
    [Route("")]
    public async Task<IActionResult> AddMerchantIntegration([FromRoute] Guid aggregateId, [FromBody] MerchantIntegration integration)
    {
        await _commandHandler.HandleAsync(new Command("AddMerchantIntegration", nameof(Aggregates.MerchantIntegration), aggregateId, null,
            data: JObject.FromObject(integration)));
        
        return Created($"merchants/{aggregateId}/integrations/{integration.Id}", aggregateId);
    }
    
    [HttpPost]
    [Route("{id}")]
    public async Task<IActionResult> UpdateMerchantIntegration([FromRoute] Guid aggregateId, [FromRoute] int id, [FromBody] MerchantIntegration integration)
    {
        integration.Id = id;
        await _commandHandler.HandleAsync(new Command("UpdateMerchantIntegration", nameof(Aggregates.MerchantIntegration), aggregateId, null,
            data: JObject.FromObject(integration)));

        return Accepted();
    }
    
    [HttpPost]
    [Route("{id}/reintegrate")]
    public async Task<IActionResult> ReintegrateMerchantIntegration([FromRoute] Guid aggregateId, [FromRoute] int id, [FromBody] MerchantIntegration integration)
    {
        integration.Id = id;
        await _commandHandler.HandleAsync(new Command("ReintegrateMerchantIntegration", nameof(Aggregates.MerchantIntegration), aggregateId, null,
            data: JObject.FromObject(integration)));

        return Accepted();
    }
    
    [HttpPost]
    [Route("{id}/delete")]
    public async Task<IActionResult> DeleteMerchantIntegration([FromRoute] Guid aggregateId, [FromRoute] int id, [FromBody] MerchantIntegration integration)
    {
        integration.Id = id;
        await _commandHandler.HandleAsync(new Command("DeleteMerchantIntegration", nameof(Aggregates.MerchantIntegration), aggregateId, null,
            data: JObject.FromObject(integration)));

        return Accepted();
    }

    [HttpGet]
    [Route("")]
    public async Task<ActionResult<IEnumerable<MerchantIntegration>>> GetIntegrations([FromRoute] Guid aggregateId)
    {
        return Ok();
    }
    
    [HttpGet]
    [Route("{id}")]
    public async Task<ActionResult<IEnumerable<MerchantIntegration>>> GetIntegrations([FromRoute] Guid aggregateId, [FromRoute] int id)
    {
        return Ok();
    }
}