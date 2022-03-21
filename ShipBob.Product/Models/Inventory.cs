namespace ShipBob.Product.Models;

public record Inventory
{
    public Guid AggregateId { get; init; } = default!;
    public int Quantity { get; init; }
}