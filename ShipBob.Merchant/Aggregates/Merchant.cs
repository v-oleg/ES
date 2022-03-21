using ES.Core;
using ES.Core.Attributes;
using ES.Core.Commands;
using ES.Core.Services.Abstractions;
using Newtonsoft.Json.Linq;

namespace ShipBob.Merchant.Aggregates;

// ReSharper disable once ClassNeverInstantiated.Global
public class Merchant : Aggregate
{
    public Merchant(IAggregateEventCreator aggregateEventCreator) : base(aggregateEventCreator)
    {
    }

    [AggregateCommandHandler("AddMerchant")]
    public void AddMerchant(Command command)
    {
        if (AggregateId != null)
        {
            throw new AggregateException("Merchant already exists.");
        }

        AddEvent(command, "MerchantAdded", new JObject());
        AddEvent(command, "MerchantInformationUpdated", data =>
        {
            data["Name"] = command.Data!["Name"];
            data["Website"] = command.Data!["Website"];
        });
        AddEvent(command, "MerchantMailingAddressUpdated", data =>
        {
            data["Address"] = command.Data!["Address"];
        });
    }
    
    [AggregateCommandHandler("UpdateMerchantInformation")]
    public void UpdateMerchantInformation(Command command)
    {
        if (AggregateId == null)
        {
            throw new AggregateException("Merchant does not exist.");
        }
        
        AddEvent(command, "MerchantInformationUpdated", data =>
        {
            data["Name"] = command.Data!["Name"];
            data["Website"] = command.Data!["Website"];
        });
    }
    
    [AggregateCommandHandler("UpdateMerchantMailingAddress")]
    public void UpdateMerchantMailingAddress(Command command)
    {
        if (AggregateId == null)
        {
            throw new AggregateException("Merchant does not exist.");
        }
        
        AddEvent(command, "MerchantMailingAddressUpdated", data =>
        {
            data["Address"] = command.Data!["Address"];
        });
    }
    
    [AggregateCommandHandler("DeactivateMerchant")]
    public void DeactivateMerchant(Command command)
    {
        if (AggregateId == null)
        {
            throw new AggregateException("Merchant does not exist.");
        }
        
        AddEvent(command, "MerchantDeactivated", new JObject());
    }
}