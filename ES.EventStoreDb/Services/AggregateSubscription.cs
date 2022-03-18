using ES.Core;
using ES.Core.Services.Abstractions;
using ES.EventStoreDb.Extensions;
using EventStore.Client;

namespace ES.EventStoreDb.Services;

internal sealed class AggregateSubscription : ISubscription
{
    private readonly IEventStoreClientService<EventStoreClient> _eventStoreClient;
    private readonly Dictionary<Type, IProjectorInformation> _aggregateProjectorInformations;
    private readonly IProjectorFactory _projectorFactory;

    // ReSharper disable once CollectionNeverUpdated.Local
    private static readonly Dictionary<string, Projector> AggregateProjectors = new();

    public AggregateSubscription(IEventStoreClientService<EventStoreClient> eventStoreClient,
        IEnumerable<IProjectorInformation> aggregateProjectorInformations, 
        IProjectorFactory projectorFactory)
    {
        _eventStoreClient = eventStoreClient;
        _aggregateProjectorInformations = aggregateProjectorInformations.ToDictionary(k => k.AggregateProjectorType);
        _projectorFactory = projectorFactory;
    }

    public async Task SubscribeAsync(params Type[] aggregateProjectors)
    {
        if (aggregateProjectors.Length == 0)
        {
            foreach (var aggregateProjectorInformation in _aggregateProjectorInformations.Values)
            {
                await SubscribeAsync(aggregateProjectorInformation);
            }
        }
        else
        {
            foreach (var aggregateProjector in aggregateProjectors)
            {
                await SubscribeAsync(_aggregateProjectorInformations[aggregateProjector]);
            }
        }
    }

    #region Helpers

    private async Task SubscribeAsync(IProjectorInformation projectorInformation)
    {
        var stream =
            Tools.Instance.Converter.ToAggregateNameStream(projectorInformation.Service,
                projectorInformation.AggregateType);

        var aggregateProjector = AggregateProjectors.ContainsKey(projectorInformation.TypeFullName)
            ? AggregateProjectors[projectorInformation.TypeFullName]
            : _projectorFactory.Create(projectorInformation.AggregateProjectorType);

        var lastEventNumber = await aggregateProjector.GetLasEventNumberAsync();
        var startFrom = lastEventNumber == null
            ? FromStream.Start
            : FromStream.After(new StreamPosition(lastEventNumber.Value));

        await _eventStoreClient.EventStoreClient.SubscribeToStreamAsync(stream, startFrom,
            async (streamSubscription, @event, cancellationToken) =>
                await HandleEventAsync(aggregateProjector, streamSubscription, @event, cancellationToken), true,
            // ReSharper disable once AsyncVoidLambda
            async (streamSubscription, dropReason, ex) =>
                await ResubscribeSync(projectorInformation, streamSubscription, dropReason, ex));
    }

    private async Task HandleEventAsync(Projector projector, StreamSubscription stream, ResolvedEvent @event, CancellationToken cancellationToken)
    {
        var aggregateEvent = @event.Event.AsAggregateEvent();
        aggregateEvent.GetEventJsonReference()["eventNumber"] = @event.Link!.EventNumber.ToUInt64();
        await projector.InitAsync(aggregateEvent.AggregateId);
        await projector.HandleAsync(aggregateEvent);
    }

    private async Task ResubscribeSync(IProjectorInformation projectorInformation,
        StreamSubscription stream, SubscriptionDroppedReason dropReason, Exception? exception)
    {
        //TODO log drop reason and exception
        await SubscribeAsync(projectorInformation);
    }

    #endregion
}