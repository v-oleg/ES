using ES.Core;
using ES.Core.Attributes;
using ES.Core.Commands;
using ES.Core.Events;
using ES.Core.Services.Abstractions;
using Newtonsoft.Json.Linq;

namespace ShipBob.Product.Aggregates;

public class Product : Aggregate
{
    private bool _deleted = false;
    
    public Product(IAggregateEventCreator aggregateEventCreator) : base(aggregateEventCreator)
    {
    }
    
    [AggregateCommandHandler("AddProduct")]
    public void AddMerchantIntegration(Command command)
    {
        if (AggregateId != null)
        {
            throw new AggregateException("Product already exists.");
        }
        
        AddEvent(command, "ProductAdded", new JObject());
        AddEvent(command, "ProductInformationUpdated", data =>
        {
            data["Title"] = command.Data!["Title"];
            data["MadeIn"] = command.Data!["MadeIn"];
        });
        AddEvent(command, "ProductPropertiesUpdated", data =>
        {
            data["Properties"] = command.Data!["Properties"];
        });
    }
    
    [AggregateCommandHandler("UpdateProductInformation")]
    public void UpdateProductInformation(Command command)
    {
        Validate();

        AddEvent(command, "ProductInformationUpdated", data =>
        {
            data["Title"] = command.Data!["Title"];
            data["MadeIn"] = command.Data!["MadeIn"];
        });
    }

    [AggregateCommandHandler("UpdateProductProperties")]
    public void UpdateProductProperties(Command command)
    {
        Validate();
        
        AddEvent(command, "ProductPropertiesUpdated", data =>
        {
            data["Properties"] = command.Data!["Properties"];
        });
    }
    
    [AggregateCommandHandler("DeleteProduct")]
    public void DeleteProduct(Command command)
    {
        Validate();
        
        AddEvent(command, "ProductDeleted", new JObject());
    }

    [AggregateEventHandler("ProductDeleted")]
    public void ProductDeleted(AggregateEvent e)
    {
        _deleted = true;
    }

    #region Helpers

    private void Validate()
    {
        if (AggregateId == null)
        {
            throw new AggregateException("Product does not exist.");
        }

        if (_deleted) throw new AggregateException("Product already deleted");
    }

    #endregion
}