using ES.Core;
using ES.Core.Attributes;
using ES.Core.Commands;
using ES.Core.Events;
using ES.Core.Services.Abstractions;
using Newtonsoft.Json.Linq;

namespace ES.EventStoreDb.Example.Aggregates;

public class People : Aggregate
{
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;

    public People(IAggregateEventCreator aggregateEventCreator) : base(aggregateEventCreator)
    {
    }

    [AggregateCommandHandler("CreatePerson")]
    public IEnumerable<AggregateEvent> CreatePerson(Command command)
    {
        if (AggregateVersion != -1)
        {
            throw new AggregateException("Person already exists");
        }
        
        //additional validation or business logic
        
        AddEvent(command, "PersonCreated", _ => { });
        AddEvent(command, "PersonNameUpdated", data =>
        {
            data["FirstName"] = command.Data!["FirstName"];
            data["LastName"] = command.Data!["LastName"];
        });
        AddEvent(command, "PersonMailingAddressUpdated", command.Data!["Address"]!.ToObject<JObject>()!);

        return EventsToWrite;
    }
    
    [AggregateCommandHandler("UpdatePersonName")]
    public IEnumerable<AggregateEvent> UpdatePersonName(Command command)
    {
        if (AggregateVersion == -1)
        {
            throw new AggregateException("Person not found");
        }

        var firstName = command.Data!["FirstName"]!.Value<string>();
        var lastName = command.Data!["LastName"]!.Value<string>();

        if (firstName == _firstName && lastName == _lastName)
        {
            return Enumerable.Empty<AggregateEvent>();
        }

        AddEvent(command, "PersonNameUpdated", data =>
        {
            data["FirstName"] = command.Data!["FirstName"];
            data["LastName"] = command.Data!["LastName"];
        });
        
        return EventsToWrite;
    }

    [AggregateCommandHandler("UpdatePersonMailingAddress")]
    public async Task<IEnumerable<AggregateEvent>> UpdatePersonMailingAddress(Command command)
    {
        if (AggregateVersion == -1)
        {
            throw new AggregateException("Person not found");
        }
        
        // some async logic
        await Task.CompletedTask;
        
        AddEvent(command, "PersonMailingAddressUpdated", command.Data!["Address"]!.ToObject<JObject>()!);

        return EventsToWrite;
    }

    [AggregateEventHandler("PersonNameUpdated")]
    public void PersonCreated(AggregateEvent @event)
    {
        _firstName = @event.Data["FirstName"]!.Value<string>()!;
        _lastName = @event.Data["LastName"]!.Value<string>()!;
    }
}