using ES.Core;
using ES.Core.Attributes;
using ES.Core.Commands;
using ES.Core.ConfigSettings;
using ES.Core.Services.Abstractions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace ShipBob.Product.Aggregates;

public class Inventory : Aggregate
{
    private readonly ServiceOptions _serviceOptions;
    private readonly IEventReader _eventReader;
    
    public Inventory(IAggregateEventCreator aggregateEventCreator, IEventReader eventReader, IOptions<ServiceOptions> options) : base(aggregateEventCreator)
    {
        _eventReader = eventReader;
        _serviceOptions = options.Value;
    }

    [AggregateCommandHandler("AddInventory")]
    public async Task AddInventory(Command command)
    {
        if (AggregateId != null)
        {
            throw new AggregateException("Inventory already exists.");
        }

        await ValidateProductAsync(command.AggregateId);
        
        AddEvent(command, "InventoryAdded", new JObject());
        AddEvent(command, "InventoryQuantityAdded", data =>
        {
            data["Quantity"] = command.Data!["Quantity"];
        });
    }
    
    [AggregateCommandHandler("AddInventoryQuantity")]
    public async Task UpdateInventoryQuantity(Command command)
    {
        await ValidateAsync(command.AggregateId);
        
        AddEvent(command, "InventoryQuantityAdded", data =>
        {
            data["Quantity"] = command.Data!["Quantity"];
        });
    }
    
    [AggregateCommandHandler("AllocateInventory")]
    public async Task AllocateInventory(Command command)
    {
        await ValidateAsync(command.AggregateId);
        
        AddEvent(command, "InventoryAllocated", new JObject());
    }
    
    [AggregateCommandHandler("DeallocateInventory")]
    public async Task DeallocateInventory(Command command)
    {
        await ValidateAsync(command.AggregateId);
        
        AddEvent(command, "InventoryDeallocated", new JObject());
    }
    
    #region Helpers

    private async Task ValidateAsync(Guid productId)
    {
        if (AggregateId == null)
        {
            throw new AggregateException("Inventory does not exist.");
        }

        await ValidateProductAsync(productId);
    }

    private async Task ValidateProductAsync(Guid productId)
    {
        var stream = Tools.Instance.Converter.ToAggregateIdStream(_serviceOptions.Name, nameof(Product), productId);
        var product = await _eventReader.GetFirstAggregateEventOrNullsAsync(stream);

        if (product == null)
        {
            throw new AggregateException($"Product id = {productId} does not exist.");
        }
    }

    #endregion
}