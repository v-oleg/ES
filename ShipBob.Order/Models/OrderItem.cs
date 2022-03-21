namespace ShipBob.Order.Models;

public record OrderItem
{
    public string ReferenceId { get; init; } = null!;
    public decimal Price { get; init; }
    public int Quantity { get; init; }
}