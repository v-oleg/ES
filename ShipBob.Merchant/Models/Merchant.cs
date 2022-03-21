namespace ShipBob.Merchant.Models;
public record Merchant
{

    public Guid AggregateId { get; init; } = default!;
    public string Name { get; init; } = null!;
    public string Website { get; init; } = null!;
    public Address Address { get; init; } = default!;
}