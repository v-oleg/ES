namespace ES.EventStoreDb.Example.Projection
{
    public class PersonAddressProjection : Core.Services.Abstractions.Projection
    {
        public string Address1 { get; set; } = string.Empty;
        public string? Address2 { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }
}
