using ES.Core.Services.Abstractions;

namespace ShipBob.CreateShipment.ProcessManager;

public class CreateShipmentPmProjection : Projection
{
    public string Step { get; set; }
    public string StepData { get; set; }
    public string Status { get; set; }
}