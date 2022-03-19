using ES.Core.Attributes;
using ES.Core.Events;
using ES.Core.Services.Abstractions;
using ES.EventStoreDb.Example.Projection;
using Newtonsoft.Json.Linq;

namespace ES.EventStoreDb.Example.Projectors
{
    [Ignore]
    public class PersonEventSourcedProjector : AggregateProjector<PersonProjection>
    {
        [AggregateEvent("PersonCreated")]
        public void PersonCreated(AggregateEvent @event)
        {
        }

        [AggregateEvent("PersonNameUpdated")]
        public void PersonNameUpdated(AggregateEvent @event)
        {
            Value.FirstName = @event.Data["FirstName"]!.Value<string>()!;
            Value.LastName = @event.Data["LastName"]!.Value<string>()!;
        }

        public override Task<ulong?> GetLasEventNumberAsync()
        {
            throw new NotImplementedException();
        }

        protected override Task UpdateLasEventNumberAsync(ulong eventNumber)
        {
            throw new NotImplementedException();
        }

        protected override Task FetchAsync()
        {
            throw new NotImplementedException();
        }

        protected override Task SaveAsync()
        {
            throw new NotImplementedException();
        }
    }
}
