namespace ShipBob.Shipment.Models;

public record ShipmentItem
{
    public string ReferenceId { get; init; } = null!;
    public decimal Price { get; init; }
    public int Quantity { get; init; }
}