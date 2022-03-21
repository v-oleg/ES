using System.Collections.Concurrent;
using ES.Core.Commands;
using ES.Core.ConfigSettings;
using ES.Core.Events;
using ES.Core.Services.Abstractions;
using Microsoft.Extensions.Options;
using ES.Core.Extensions;

namespace ES.Core.Services;

public sealed class CommandHandler : ICommandHandler
{
    private readonly IEventReader _eventReader;
    private readonly IEventWriter _eventWriter;
    private readonly IAggregateFactory _aggregateFactory;
    private readonly ServiceOptions _serviceOptions;

    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> CommandsHandled = new();

    public CommandHandler(IEventReader eventReader, IEventWriter eventWriter, IAggregateFactory aggregateFactory,
        IOptions<ServiceOptions> options)
    {
        _eventReader = eventReader;
        _eventWriter = eventWriter;
        _aggregateFactory = aggregateFactory;
        
        _serviceOptions = options.Value;
    }

    public async Task<IEnumerable<AggregateEvent>> HandleAsync(Command command, TimeSpan? timeout = null)
    {
        // Lock to make handling commands mutually exclusive by CommandId
        var commandIdLock =
            CommandsHandled.AddOrUpdate(command.CommandId, new SemaphoreSlim(1, 1), (_, idLock) => idLock);
        await commandIdLock.WaitAsync();

        try
        {
            var aggregate = _aggregateFactory.Create(command.AggregateType);
            
            var stream = Tools.Instance.Converter.ToAggregateIdStream(_serviceOptions.Name, command.AggregateType,
                command.AggregateId);
            var events = (await _eventReader.GetAggregateEventsAsync(stream)).ToList();
            aggregate.AggregateId = events.Count > 0 ? command.AggregateId : null;
            //command must be idempotent. if exists return associated events
            if (!command.IsIdempotent(events))
            {
                return events.Where(e => e.CommandId == command.CommandId);
            }

            aggregate.ApplyEvents(events);
            var eventsToWrite = (await aggregate.HandleCommand(command)).ToList();
            await _eventWriter.WriteEventsAsync(stream, eventsToWrite);
            aggregate.EventsToWrite.Clear();
            
            return eventsToWrite;
        }
        finally
        {
            commandIdLock.Release();
        }
    }
}