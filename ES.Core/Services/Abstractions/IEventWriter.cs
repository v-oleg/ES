using ES.Core.Events;

namespace ES.Core.Services.Abstractions;

public interface IEventWriter
{
    Task WriteEventsAsync(string stream, IEnumerable<AggregateEvent> events);
}