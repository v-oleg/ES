using EventStore.Client;
using ES.Core.Events;
using ES.Core.Services.Abstractions;

namespace ES.EventStoreDb.Services;

internal class EventWriter : IEventWriter
{
    private readonly IEventStoreClientService<EventStoreClient> _eventStoreClient;

    public EventWriter(IEventStoreClientService<EventStoreClient> eventStoreClient)
    {
        _eventStoreClient = eventStoreClient;
    }

    public async Task WriteEventsAsync(string stream, IEnumerable<AggregateEvent> events)
    {
        var eventData = events.Select(e => new EventData(Uuid.NewUuid(), e.EventName, e.GetData(), e.GetMetadata()));
        await _eventStoreClient.EventStoreClient.AppendToStreamAsync(stream, StreamState.Any, eventData);
    }
}