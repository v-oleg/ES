using System.ComponentModel.DataAnnotations;

namespace ES.EventStoreDb.Example.Data;

public class People
{
    [Key]
    public Guid AggregateId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Address1 { get; set; }
    public string? Address2 { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
    public string Country { get; set; }
}