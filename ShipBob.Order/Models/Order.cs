namespace ShipBob.Order.Models;

public record Order
{
    public Guid AggregateId { get; init; } = default!;
    public decimal TotalPrice { get; init; }
    public FinancialStatus FinancialStatus { get; init; }
    public Address ShippingAddress { get; init; } = default!;
    public IEnumerable<OrderItem> OrderItems { get; init; } = default!;
}