using EventStore.Client;
using Microsoft.Extensions.Options;
using ES.Core.Services.Abstractions;
using ES.EventStoreDb.ConfigSettings;

namespace ES.EventStoreDb.Services;

internal sealed class EventStoreClientService : IEventStoreClientService<EventStoreClient>
{
    public EventStoreClient EventStoreClient { get; }

    public EventStoreClientService(IOptions<EventStoreOptions> options)
    {
        var settings = EventStoreClientSettings.Create(options.Value.ConnectionString);
        EventStoreClient = new EventStoreClient(settings);
    }
}