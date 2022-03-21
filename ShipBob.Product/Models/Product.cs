using Newtonsoft.Json.Linq;

namespace ShipBob.Product.Models;

public record Product
{
    public Guid AggregateId { get; init; } = default!;
    public Guid MerchantId { get; set; } = default!;
    public string Title { get; init; } = null!;
    public string MadeIn { get; init; } = null!;
    public JObject Properties { get; init; } = default!;
}