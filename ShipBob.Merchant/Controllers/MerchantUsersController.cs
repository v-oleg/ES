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
[Route("merchants/{aggregateId}/users")]
public class MerchantUsersController : ControllerBase
{
    private readonly ICommandHandler _commandHandler;
    private readonly IMongoCollection<BsonDocument> _peopleCollection;
    private readonly IEventReader _eventReader;
    private readonly ServiceOptions _serviceOptions;

    public MerchantUsersController(ICommandHandler commandHandler,
        IEventReader eventReader, IOptions<ServiceOptions> options)
    {
        _commandHandler = commandHandler;
        _eventReader = eventReader;
        _serviceOptions = options.Value;
    }

    [HttpPost]
    [Route("")]
    public async Task<IActionResult> AddMerchantUser([FromRoute] Guid aggregateId, [FromBody] MerchantUser user)
    {
        await _commandHandler.HandleAsync(new Command("AddMerchantUser", nameof(Aggregates.MerchantUser), aggregateId, null,
            data: JObject.FromObject(user)));
        
        return Created($"merchants/{aggregateId}/users/{user.Id}", user.AggregateId);
    }

    [HttpPost]
    [Route("{id}")]
    public async Task<IActionResult> UpdateMerchantUserInformation([FromRoute] Guid aggregateId, [FromRoute] int id,
        [FromBody] MerchantUser user)
    {
        user.Id = id;
        await _commandHandler.HandleAsync(new Command("UpdateMerchantUserInformation", nameof(Aggregates.MerchantUser), aggregateId, null,
            data: JObject.FromObject(user)));

        return Accepted();
    }
    
    [HttpPost]
    [Route("{id}/assignowner")]
    public async Task<IActionResult> AssignMerchantUserOwner([FromRoute] Guid aggregateId, [FromRoute] int id)
    {
        await _commandHandler.HandleAsync(new Command("AssignMerchantUserOwner", nameof(Aggregates.MerchantUser), aggregateId, null, data: new JObject
        {
            ["Id"] = id
        }));

        return Accepted();
    }
    
    [HttpPost]
    [Route("{id}/unassignowner")]
    public async Task<IActionResult> UnassignMerchantUserOwner([FromRoute] Guid aggregateId, [FromRoute] int id)
    {
        await _commandHandler.HandleAsync(new Command("UnassignMerchantUserOwner", nameof(Aggregates.MerchantUser), aggregateId, null, data: new JObject
        {
            ["Id"] = id
        }));

        return Accepted();
    }
    
    [HttpPost]
    [Route("{id}/delete")]
    public async Task<IActionResult> DeleteUser([FromRoute] Guid aggregateId, [FromRoute] int id)
    {
        await _commandHandler.HandleAsync(new Command("DeleteMerchantUser", nameof(Aggregates.MerchantUser), aggregateId, null, data: new JObject
        {
            ["Id"] = id
        }));

        return Accepted();
    }
    
    [HttpGet]
    [Route("")]
    public async Task<ActionResult<IEnumerable<MerchantUser>>> GetUsers([FromRoute] Guid aggregateId)
    {
        return Ok();
    }
    
    [HttpGet]
    [Route("{id}")]
    public async Task<ActionResult<IEnumerable<MerchantUser>>> GetUser([FromRoute] Guid aggregateId, [FromRoute] int id)
    {
        return Ok();
    }
}