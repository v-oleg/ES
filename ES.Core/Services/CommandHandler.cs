using System.Collections.Concurrent;
using ES.Core.Commands;
using ES.Core.ConfigSettings;
using ES.Core.Events;
using ES.Core.Services.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ES.Core.Extensions;

namespace ES.Core.Services;

public sealed class CommandHandler : ICommandHandler
{
    private readonly IEventReader _eventReader;
    private readonly IEventWriter _eventWriter;
    private readonly IAggregateFactory _aggregateFactory;

    private readonly ILogger<CommandHandler> _logger;
    private readonly ServiceOptions _serviceOptions;

    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> CommandsHandled = new();

    public CommandHandler(IEventReader eventReader, IEventWriter eventWriter, IAggregateFactory aggregateFactory,
        ILogger<CommandHandler> logger, IOptions<ServiceOptions> options)
    {
        _eventReader = eventReader;
        _eventWriter = eventWriter;
        _aggregateFactory = aggregateFactory;
        
        _logger = logger;
        _serviceOptions = options.Value;
    }

    public async Task<IEnumerable<AggregateEvent>> HandleAsync(Command command, TimeSpan? timeout = null)
    {
        // Lock to make handling commands mutually exclusive by CommandId
        var commandIdLock =
            CommandsHandled.AddOrUpdate(command.CommandId, new SemaphoreSlim(1, 1), (_, idLock) => idLock);
        await commandIdLock.WaitAsync().ConfigureAwait(false);

        try
        {
            var aggregate = _aggregateFactory.Create(command.AggregateType);
            
            var stream = Tools.Instance.Converter.ToAggregateIdStream(_serviceOptions.Name, command.AggregateType,
                command.AggregateId);
            var events = (await _eventReader.GetAggregateEventsAsync(stream)).ToList();

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
        catch (Exception e)
        {
            _logger.LogError(e,
                "Error while executing a command (CommandName: {CommandName}, CommandId: {CommandId}, " +
                "CorrelationId: {CorrelationId}, CausationId: {CausationId}, AggregateId: {AggregateId}, " +
                "AggregateType: {AggregateType}, CommandVersion: {CommandVersion}, AggregateVersion: {AggregateVersion}" +
                "AuthorizedUserId: {AuthorizedUserId}, CommandData: {CommandData})",
                command.CommandName, command.CommandId, command.CorrelationId, command.CausationId, 
                command.AggregateId, command.AggregateType, command.CommandVersion, command.AggregateVersion, 
                command.AuthorizedUserId, command.Data);
            throw;
        }
        finally
        {
            commandIdLock.Release();
        }
    }
}