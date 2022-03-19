using System.ComponentModel.DataAnnotations;

namespace ES.EventStoreDb.Example.Sql;

public class PersonMailingAddress
{
    [Key]
    public Guid AggregateId { get; set; }
    public string Address1 { get; set; } = null!;
    public string? Address2 { get; set; }
    public string City { get; set; } = null!;
    public string State { get; set; } = null!;
    public string ZipCode { get; set; } = null!;
    public string Country { get; set; } = null!;
}