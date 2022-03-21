namespace ShipBob.Shipment.Models;

public record Shipment
{
    public Guid AggregateId { get; init; } = default!;
    public Guid OrderId { get; init; } = default!;
    public IEnumerable<ShipmentItem> ShipmentItems { get; init; } = default!;
    public Address ShippingAddress { get; init; } = default!;
}