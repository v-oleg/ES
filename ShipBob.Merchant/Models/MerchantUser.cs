namespace ShipBob.Merchant.Models;

public record MerchantUser
{
    public Guid AggregateId { get; init; } = default!;
    public int Id { get; set; }
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public bool Owner { get; set; } = default!;
}