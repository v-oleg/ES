using ES.Core.Commands;
using ES.Core.Events;

namespace ES.Core.Services.Abstractions;

public interface ICommandHandler
{
    Task<IEnumerable<AggregateEvent>> HandleAsync(Command command, TimeSpan? timeout = null);
}