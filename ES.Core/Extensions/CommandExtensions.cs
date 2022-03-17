using ES.Core.Commands;
using ES.Core.Events;

namespace ES.Core.Extensions;

public static class CommandExtensions
{
    public static bool IsIdempotent(this Command command, IEnumerable<AggregateEvent> events) =>
        events.All(e => e.CommandId != command.CommandId);
}