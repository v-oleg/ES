namespace ShipBob.Merchant.Models;

public record MerchantIntegration
{
    public Guid Guid { get; init; } = default!;
    public int Id { get; set; }
    public Platform Platform { get; init; }
    public string StoreUrl { get; init; } = null!;
    public string Token { get; init; } = null!;
}