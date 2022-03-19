using System.Reflection;
using ES.Core.Attributes;
using ES.Core.Services.Abstractions;
using ES.EventStoreDb.Extensions;
using EventStore.Client;

namespace ES.EventStoreDb.Services;

internal class AllStreamsSubscription : ISubscription
{
    private readonly IEventStoreClientService<EventStoreClient> _eventStoreClient;
    private readonly Dictionary<Type, IProjectorInformation> _projectorInformations;
    private readonly IProjectorFactory _projectorFactory;

    // ReSharper disable once CollectionNeverUpdated.Local
    private static readonly Dictionary<string, Projector> Projectors = new();

    public AllStreamsSubscription(IEventStoreClientService<EventStoreClient> eventStoreClient,
        IEnumerable<IProjectorInformation> projectorInformations,
        IProjectorFactory projectorFactory)
    {
        _eventStoreClient = eventStoreClient;
        _projectorInformations = projectorInformations.ToDictionary(k => k.ProjectorType);
        _projectorFactory = projectorFactory;
    }

    public async Task SubscribeAsync(params Type[] projectors)
    {
        if (projectors.Length == 0)
        {
            foreach (var projectorInformation in _projectorInformations.Values.Where(p =>
                         p.ProjectorType.GetCustomAttribute(typeof(AllStreamsAttribute)) != null))
            {
                await SubscribeAsync(projectorInformation);
            }
        }
        else
        {
            foreach (var projector in projectors.Where(p => p.GetCustomAttribute(typeof(AllStreamsAttribute)) != null))
            {
                await SubscribeAsync(_projectorInformations[projector]);
            }
        }
    }

    #region Helpers

    private async Task SubscribeAsync(IProjectorInformation projectorInformation)
    {
        var projector = Projectors.ContainsKey(projectorInformation.TypeFullName)
            ? Projectors[projectorInformation.TypeFullName]
            : _projectorFactory.Create(projectorInformation.ProjectorType);

        var lastEventNumber = await projector.GetLasEventNumberAsync();
        var startFrom = lastEventNumber == null
            ? FromAll.Start
            : FromAll.After(new Position(lastEventNumber.Value, lastEventNumber.Value));

        await _eventStoreClient.EventStoreClient.SubscribeToAllAsync(startFrom,
            async (streamSubscription, @event, cancellationToken) =>
                await HandleEventAsync(projector, streamSubscription, @event, cancellationToken), true,
            // ReSharper disable once AsyncVoidLambda
            async (streamSubscription, dropReason, ex) =>
                await ResubscribeSync(projectorInformation, streamSubscription, dropReason, ex),
            filterOptions: new SubscriptionFilterOptions(EventTypeFilter.ExcludeSystemEvents()));
    }

    private async Task HandleEventAsync(Projector projector, StreamSubscription stream, ResolvedEvent @event,
        CancellationToken cancellationToken)
    {
        var e = @event.Event.AsEvent();

        e.GetEventJsonReference()["eventNumber"] = @event.Event.Position.CommitPosition;
        await projector.InitAsync(e.AsAggregateEvent().AggregateId);
        await projector.HandleAsync(e);
    }

    private async Task ResubscribeSync(IProjectorInformation projectorInformation,
        StreamSubscription stream, SubscriptionDroppedReason dropReason, Exception? exception)
    {
        //TODO log drop reason and exception
        await SubscribeAsync(projectorInformation);
    }

    #endregion
}