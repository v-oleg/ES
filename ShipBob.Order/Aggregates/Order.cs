using ES.Core;
using ES.Core.Attributes;
using ES.Core.Commands;
using ES.Core.Events;
using ES.Core.Services.Abstractions;
using Newtonsoft.Json.Linq;

namespace ShipBob.Order.Aggregates;

public class Order : Aggregate
{
    private Guid _merchantId;
    
    private decimal _totalPrice = default;
    private string _financialStatus;
    private readonly Dictionary<string, int> _orderItems = new ();

    private readonly IEventReader _eventReader;

    public Order(IAggregateEventCreator aggregateEventCreator, IEventReader eventReader) : base(aggregateEventCreator)
    {
        _eventReader = eventReader;
    }

    [AggregateCommandHandler("IngestOrder")]
    public void IngestOrder(Command command)
    {
        if (AggregateId != null)
        {
            throw new AggregateException("Order already exists.");
        }

        var merchantId = command.Data!["MerchantId"]!.ToObject<Guid>();
        command.CorrelationId = merchantId;
        AddEvent(command, "OrderIngested", data =>
        {
            data["MerchantId"] = merchantId;
        });
        AddEvent(command, "OrderFinancialInformationUpdated", data =>
        {
            data["TotalPrice"] = command.Data!["TotalPrice"];
            data["FinancialStatus"] = command.Data!["FinancialStatus"];
        });
        AddEvent(command, "OrderShippingAddressUpdated", data =>
        {
            data["ShippingAddress"] = command.Data!["ShippingAddress"];
        });
        AddEvent(command, "OrderItemsAdded", data =>
        {
            data["OrderItems"] = command.Data!["OrderItems"];
        });
    }
    
    [AggregateCommandHandler("UpdateOrderShippingAddress")]
    public void UpdateOrderShippingAddress(Command command)
    {
        if (AggregateId == null)
        {
            throw new AggregateException("Order does not exist.");
        }
        command.CorrelationId = _merchantId;
        AddEvent(command, "OrderShippingAddressUpdated", data =>
        {
            data["ShippingAddress"] = command.Data!["ShippingAddress"];
        });
    }
    
    [AggregateCommandHandler("UpdateOrderFinancialInformation")]
    public void UpdateOrderFinancialInformation(Command command)
    {
        if (AggregateId == null)
        {
            throw new AggregateException("Order does not exist.");
        }
        
        command.CorrelationId = _merchantId;
        AddEvent(command, "OrderFinancialInformationUpdated", data =>
        {
            data["TotalPrice"] = command.Data!["TotalPrice"];
            data["FinancialStatus"] = command.Data!["FinancialStatus"];
        });
    }
    
    [AggregateCommandHandler("AddOrderItem")]
    public void AddOrderItem(Command command)
    {
        if (AggregateId == null)
        {
            throw new AggregateException("Order does not exist.");
        }

        command.CorrelationId = _merchantId;
        var itemPrice = command.Data!["Price"]!.Value<decimal>();
        AddEvent(command, "OrderItemAdded", data =>
        {
            data["Price"] = itemPrice;
            data["ReferenceId"] = command.Data["ReferenceId"];
            data["Quantity"] = command.Data["Quantity"];
        });

        if (itemPrice != 0)
        {
            AddEvent(command, "OrderFinancialInformationUpdated", data =>
            {
                data["TotalPrice"] = itemPrice + _totalPrice;
                data["FinancialStatus"] = _financialStatus;
            });
        }
    }
    
    [AggregateCommandHandler("DeleteOrderItem")]
    public void DeleteOrderItems(Command command)
    {
        if (AggregateId == null)
        {
            throw new AggregateException("Order does not exist.");
        }
        
        var itemPrice = command.Data!["Price"]!.Value<decimal>();
        var quantity = command.Data["Quantity"]!.Value<int>();
        var refId = command.Data["ReferenceId"]!.Value<string>()!;
        
        if (!_orderItems.ContainsKey(refId))
        {
            throw new AggregateException($"Order item {refId} does not exist.");
        }
        
        if (quantity < 1)
        {
            throw new Exception("Quantity must be greater than zero.");
        }

        if (_orderItems[refId] < quantity)
        {
            quantity = _orderItems[refId];
        }
        
        command.CorrelationId = _merchantId;
        AddEvent(command, "OrderItemDeleted", data =>
        {
            data["Price"] = itemPrice;
            data["ReferenceId"] = command.Data["ReferenceId"];
            data["Quantity"] = quantity;
        });
        
        if (itemPrice != 0)
        {
            AddEvent(command, "OrderFinancialInformationUpdated", data =>
            {
                data["TotalPrice"] = _totalPrice - itemPrice;
                data["FinancialStatus"] = _financialStatus;
            });
        }
    }

    [AggregateEventHandler("OrderFinancialInformationUpdated")]
    public void OrderFinancialInformationUpdated(AggregateEvent e)
    {
        _totalPrice = e.Data["TotalPrice"]!.Value<decimal>();
        _financialStatus = e.Data["FinancialStatus"]!.Value<string>()!;
    }

    [AggregateEventHandler("OrderItemAdded")]
    public void OrderItemAdded(AggregateEvent e)
    {
        var refId = e.Data["ReferenceId"]!.Value<string>()!;
        var quantity = e.Data["Quantity"]!.Value<int>();
        if (_orderItems.ContainsKey(refId))
        {
            _orderItems[refId] += quantity;
        }
        else
        {
            _orderItems[refId] = quantity;
        }
    }

    [AggregateEventHandler("OrderIngested")]
    public void OrderIngested(AggregateEvent e)
    {
        _merchantId = e.Data["MerchantId"]!.ToObject<Guid>();
    }
    
    [AggregateEventHandler("OrderItemDeleted")]
    public void OrderItemDeleted(AggregateEvent e)
    {
        var refId = e.Data["ReferenceId"]!.Value<string>()!;
        var quantity = e.Data["Quantity"]!.Value<int>();
        if (_orderItems.ContainsKey(refId))
        {
            _orderItems[refId] -= quantity;
        }
    }

    #region Helper

    private async Task ValidateMerchant(Guid merchantId)
    {
        var stream = Tools.Instance.Converter.ToAggregateIdStream("merchant", "Merchant", merchantId);
        var merchant = await _eventReader.GetFirstAggregateEventOrNullsAsync(stream);

        if (merchant == null)
        {
            throw new AggregateException($"Merchant {merchantId} does not exist");
        }
    }

    #endregion
}