namespace ES.EventStoreDb.ConfigSettings;

internal sealed class EventStoreOptions
{
    public const string EventStoreSection = "EventStore";

    public string ConnectionString { get; set; } = string.Empty;
}