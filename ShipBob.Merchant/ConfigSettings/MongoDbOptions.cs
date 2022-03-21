namespace ShipBob.Merchant.ConfigSettings;

public sealed class MongoDbOptions
{
    public const string MongoDbSection = "MongoDb";

    public string ConnectionString { get; set; } = string.Empty;
}