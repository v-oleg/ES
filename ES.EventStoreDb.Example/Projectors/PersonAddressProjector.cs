using ES.Core.Attributes;
using ES.Core.Events;
using ES.Core.Services.Abstractions;
using ES.EventStoreDb.Example.Aggregates;
using ES.EventStoreDb.Example.Projection;
using ES.EventStoreDb.Example.Sql;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace ES.EventStoreDb.Example.Projectors
{
    [AggregateStream("merchant", nameof(People))]
    public class PersonAddressProjector : AggregateProjector<PersonAddressProjection>
    {
        private readonly SqlDbContext _dbContext;

        public PersonAddressProjector(SqlDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [AggregateEvent("PersonMailingAddressUpdated")]
        public void UpdatePersonMailingAddress(AggregateEvent @event)
        {
            Value.Address1 = @event.Data["Address1"]!.Value<string>()!;
            Value.Address2 = @event.Data["Address2"]!.Value<string>()!;
            Value.City = @event.Data["City"]!.Value<string>()!;
            Value.State = @event.Data["State"]!.Value<string>()!;
            Value.ZipCode = @event.Data["ZipCode"]!.Value<string>()!;
            Value.Country = @event.Data["Country"]!.Value<string>()!;
        }

        public override async Task<ulong?> GetLasEventNumberAsync()
        {
            try
            {
                return (await _dbContext.Checkpoints.SingleOrDefaultAsync(x => x.Projector == GetType().FullName))
                    ?.LastEventNumber ?? null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }

        protected override async Task UpdateLasEventNumberAsync(ulong eventNumber)
        {
            var checkpoint =
                await _dbContext.Checkpoints.SingleOrDefaultAsync(x => x.Projector == GetType().FullName);

            if (checkpoint == null)
            {
                checkpoint = new Checkpoints();
                _dbContext.Entry(checkpoint).State = EntityState.Added;
            }
            else
            {
                _dbContext.Entry(checkpoint).State = EntityState.Modified;
            }

            checkpoint.LastEventNumber = eventNumber;
            checkpoint.Projector = GetType().FullName!;
            await _dbContext.SaveChangesAsync();
        }

        protected override async Task FetchAsync()
        {
            var personMailingAddress =
                await _dbContext.PersonMailingAddress.SingleOrDefaultAsync(x => x.AggregateId == Value.AggregateId);

            if (personMailingAddress != null)
            {
                Value.Address1 = personMailingAddress.Address1;
                Value.Address2 = personMailingAddress.Address2;
                Value.City = personMailingAddress.City; 
                Value.State = personMailingAddress.State;
                Value.ZipCode = personMailingAddress.ZipCode;
                Value.Country = personMailingAddress.Country;
            }
        }

        protected override async Task SaveAsync()
        {
            try
            {
                var personMailingAddress =
                    await _dbContext.PersonMailingAddress.SingleOrDefaultAsync(x => x.AggregateId == Value.AggregateId);

                if (personMailingAddress == null)
                {
                    personMailingAddress = new PersonMailingAddress();
                    _dbContext.Entry(personMailingAddress).State = EntityState.Added;
                }
                else
                {
                    _dbContext.Entry(personMailingAddress).State = EntityState.Modified;    
                }

                personMailingAddress.Address1 = Value.Address1;
                personMailingAddress.Address2 = Value.Address2;
                personMailingAddress.City = Value.City;
                personMailingAddress.State = Value.State;
                personMailingAddress.ZipCode = Value.ZipCode;
                personMailingAddress.Country = Value.Country;
                personMailingAddress.AggregateId = Value.AggregateId;

                await _dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }
    }
}
