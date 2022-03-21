namespace ShipBob.Order.Models;

public record Address
{
    public string Address1 { get; init; } = null!;
    public string? Address2 { get; init; }
    public string City { get; init; } = null!;
    public string State { get; init; } = null!;
    public string ZipCode { get; init; } = null!;
    public string Country { get; init; } = null!;
}