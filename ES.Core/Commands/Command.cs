using ES.Core.Events;
using Newtonsoft.Json.Linq;

namespace ES.Core.Commands;

public class Command
{
    public string AggregateType { get; set; }
    public Guid AggregateId { get; set; }
    public long? AggregateVersion { get; set; }
    public string CommandName { get; set; }
    public Guid CommandId { get; set; }
    public int CommandVersion { get; set; }
    public DateTime CommandDate { get; set; }
    public Guid? CorrelationId { get; set; }
    public Guid? CausationId { get; set; }
    public string? AuthorizedUserId { get; set; }

    public JObject? Data { get; set; }

    public Command(string commandName, string aggregateType, Guid aggregateId, long? aggregateVersion,
        int commandVersion = 1, JObject? data = null, Event? causedBy = null, string? authorizedUserId = null)
    {
        AggregateType = aggregateType;
        AggregateId = aggregateId;
        AggregateVersion = aggregateVersion;
        CommandName = commandName;
        CommandId = Guid.NewGuid();
        CommandVersion = commandVersion;
        CommandDate = DateTime.UtcNow;
        CausationId = causedBy?.EventId;
        Data = data;
        AuthorizedUserId = authorizedUserId;
    }
}