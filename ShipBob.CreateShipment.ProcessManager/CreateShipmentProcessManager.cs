using ES.Core.Attributes;
using ES.Core.Events;
using ES.Core.Services.Abstractions;

namespace ShipBob.CreateShipment.ProcessManager;

[AllStreams]
public class CreateShipmentProcessManager : AllStreamsProjector<CreateShipmentPmProjection>
{
    [Stream("shipment", "ShipmentAdded")]
    public void ShipmentAdded(Event e)
    {
        //TODO send command to allocate inventory
    }
    
    [Stream("inventory", "InventoryAllocated")]
    public void InventoryAllocated(Event e)
    {
        //TODO send command to update order notes inventory allocated
    }
    
    [Stream("order", "OrderNotesUpdated")]
    public void OrderNotesUpdated(Event e)
    {
        //TODO send command to create label for shipment
    }
    
    [Stream("label", "LabelCreated")]
    public void LabelCreated(Event e)
    {
        
    }
    
    [Stream("shipment", "ShipmentShipped")]
    public void ShipmentShipped(Event e)
    {
        // update status PM is completed
    }
    
    public override Task InitAsync(Event e)
    {
        throw new NotImplementedException();
    }

    public override Task<ulong?> GetLasEventNumberAsync()
    {
        throw new NotImplementedException();
    }

    protected override Task UpdateLasEventNumberAsync(ulong eventNumber)
    {
        throw new NotImplementedException();
    }

    protected override Task FetchAsync()
    {
        throw new NotImplementedException();
    }

    protected override Task SaveAsync()
    {
        throw new NotImplementedException();
    }
}