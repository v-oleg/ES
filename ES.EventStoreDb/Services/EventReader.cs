using EventStore.Client;
using ES.Core.Events;
using ES.Core.Services.Abstractions;
using ES.EventStoreDb.Extensions;

namespace ES.EventStoreDb.Services;

internal sealed class EventReader : IEventReader
{
    private readonly IEventStoreClientService<EventStoreClient> _eventStoreClient;

    public EventReader(IEventStoreClientService<EventStoreClient> eventStoreClient)
    {
        _eventStoreClient = eventStoreClient;
    }

    public async Task<IEnumerable<AggregateEvent>> GetAggregateEventsAsync(string stream)
    {
        var streamResult =
            _eventStoreClient.EventStoreClient.ReadStreamAsync(Direction.Forwards, stream, StreamPosition.Start);

        var state = await streamResult.ReadState;
        return state == ReadState.StreamNotFound
            ? Enumerable.Empty<AggregateEvent>()
            : (await streamResult.ToListAsync()).Select(e => e.Event.AsAggregateEvent());
    }

    public async Task<AggregateEvent?> GetFirstAggregateEventOrNullsAsync(string stream)
    {
        var streamResult =
            _eventStoreClient.EventStoreClient.ReadStreamAsync(Direction.Forwards, stream, StreamPosition.Start, 1);
        var state = await streamResult.ReadState;
        return state == ReadState.StreamNotFound
            ? null
            : (await streamResult.FirstAsync()).Event.AsAggregateEvent();
    }
}