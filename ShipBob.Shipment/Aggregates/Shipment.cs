using ES.Core;
using ES.Core.Attributes;
using ES.Core.Commands;
using ES.Core.Events;
using ES.Core.Services.Abstractions;
using Newtonsoft.Json.Linq;

namespace ShipBob.Shipment.Aggregates;

public class Shipment : Aggregate
{
    private Guid _orderId;
    private bool _shipped;
    
    private readonly Dictionary<string, int> _shipmentItems = new ();
    private readonly IEventReader _eventReader;

    public Shipment(IAggregateEventCreator aggregateEventCreator, IEventReader eventReader) : base(aggregateEventCreator)
    {
        _eventReader = eventReader;
    }

    [AggregateCommandHandler("AddShipment")]
    public async Task ShipmentAdded(Command command)
    {
        if (AggregateId != null)
        {
            throw new AggregateException("Shipment already exists");
        }

        var orderId = command.Data!["OrderId"]!.ToObject<Guid>();

        await ValidateOrderAsync(orderId);
        command.CorrelationId = orderId;
        AddEvent(command, "ShipmentAdded", data =>
        {
            data["OrderId"] = orderId;
        });
        AddEvent(command, "ShipmentItemsAdded", data =>
        {
            data["ShipmentItems"] = command.Data!["ShipmentItems"];
        });
        AddEvent(command, "ShipmentShippingAddressUpdated", data =>
        {
            data["ShippingAddress"] = command.Data!["ShippingAddress"];
        });
    }
    
    [AggregateCommandHandler("UpdateShipmentShippingAddress")]
    public async Task UpdateShipmentShippingAddress(Command command)
    {
        if (AggregateId == null)
        {
            throw new AggregateException("Shipment already exists");
        }
        
        await ValidateOrderAsync(_orderId);
        ValidateShipment();
        
        command.CorrelationId = _orderId;
        AddEvent(command, "ShipmentShippingAddressUpdated", data =>
        {
            data["ShippingAddress"] = command.Data!["ShippingAddress"];
        });
    }
    
    [AggregateCommandHandler("AddShipmentItem")]
    public async Task AddShipmentOrderItem(Command command)
    {
        if (AggregateId == null)
        {
            throw new AggregateException("Shipment already exists");
        }
        
        await ValidateOrderAsync(_orderId);
        ValidateShipment();
        
        command.CorrelationId = _orderId;
        var quantity = command.Data!["Quantity"]!.Value<int>();
        if (quantity < 1)
        {
            throw new Exception("Quantity must be greater than zero.");
        }
        
        AddEvent(command, "ShipmentItemAdded", data =>
        {
            data["Price"] = command.Data!["Price"];
            data["ReferenceId"] = command.Data["ReferenceId"];
            data["Quantity"] = quantity;
        });
    }
    
    [AggregateCommandHandler("DeleteShipmentItem")]
    public async Task DeleteShipmentOrderItem(Command command)
    {
        if (AggregateId == null)
        {
            throw new AggregateException("Shipment already exists");
        }
        
        await ValidateOrderAsync(_orderId);
        ValidateShipment();
        
        var quantity = command.Data!["Quantity"]!.Value<int>();
        var refId = command.Data["ReferenceId"]!.Value<string>()!;
        
        if (!_shipmentItems.ContainsKey(refId))
        {
            throw new AggregateException($"Shipment item {refId} does not exist.");
        }
        
        if (quantity < 1)
        {
            throw new Exception("Quantity must be greater than zero.");
        }
        
        if (_shipmentItems[refId] < quantity)
        {
            quantity = _shipmentItems[refId];
        }
        
        command.CorrelationId = _orderId;
        AddEvent(command, "ShipmentItemDeleted", data =>
        {
            data["Price"] = command.Data!["Price"];
            data["ReferenceId"] = refId;
            data["Quantity"] = quantity;
        });
    }
    
    [AggregateCommandHandler("ShipShipment")]
    public async Task ShipShipment(Command command)
    {
        if (AggregateId == null)
        {
            throw new AggregateException("Shipment already exists");
        }
        
        await ValidateOrderAsync(_orderId);
        ValidateShipment();
        command.CorrelationId = _orderId;
        AddEvent(command, "ShipmentShipped", new JObject());
    }
    
    [AggregateEventHandler("ShipmentItemAdded")]
    public void OrderItemAdded(AggregateEvent e)
    {
        var refId = e.Data["ReferenceId"]!.Value<string>()!;
        var quantity = e.Data["Quantity"]!.Value<int>();
        if (_shipmentItems.ContainsKey(refId))
        {
            _shipmentItems[refId] += quantity;
        }
        else
        {
            _shipmentItems[refId] = quantity;
        }
    }
    
    [AggregateEventHandler("ShipmentItemDeleted")]
    public void OrderItemDeleted(AggregateEvent e)
    {
        var refId = e.Data["ReferenceId"]!.Value<string>()!;
        var quantity = e.Data["Quantity"]!.Value<int>();
        if (_shipmentItems.ContainsKey(refId))
        {
            _shipmentItems[refId] -= quantity;
        }
    }
    
    [AggregateEventHandler("ShipmentAdded")]
    public void ShipmentAdded(AggregateEvent e)
    {
        _orderId = e.Data["OrderId"]!.ToObject<Guid>();
    }
    
    [AggregateEventHandler("ShipmentShipped")]
    public void ShipmentShipped(AggregateEvent e)
    {
        _shipped = true;
    }

    #region Helpers

    private async Task ValidateOrderAsync(Guid orderId)
    {
        var stream = Tools.Instance.Converter.ToAggregateIdStream("order", "Order", orderId);
        var order = await _eventReader.GetFirstAggregateEventOrNullsAsync(stream);

        if (order == null)
        {
            throw new AggregateException($"Order {orderId} does not exist");
        }
    }

    private void ValidateShipment()
    {
        if (_shipped)
        {
            throw new AggregateException("Shipment already shipped");
        }
    }

    #endregion
}