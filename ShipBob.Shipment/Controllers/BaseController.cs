using ES.Core.Commands;
using ES.Core.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace ShipBob.Shipment.Controllers;

public class BaseController : ControllerBase
{
    private readonly ICommandHandler _commandHandler;

    public BaseController(ICommandHandler commandHandler)
    {
        _commandHandler = commandHandler;
    }

    [HttpPost]
    [Route("command")]
    public async Task<IActionResult> Command(Command command)
    {
        await _commandHandler.HandleAsync(command);

        return Accepted();
    }
}