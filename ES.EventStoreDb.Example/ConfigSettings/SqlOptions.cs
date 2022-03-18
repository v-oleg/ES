namespace ES.EventStoreDb.Example.ConfigSettings;

public sealed class SqlOptions
{
    public const string SqlSection = "Sql";

    public string ConnectionString { get; set; } = string.Empty;
}