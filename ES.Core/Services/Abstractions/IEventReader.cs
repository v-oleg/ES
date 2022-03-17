using ES.Core.Events;

namespace ES.Core.Services.Abstractions;

public interface IEventReader
{
    Task<IEnumerable<AggregateEvent>> GetAggregateEventsAsync(string stream);
}