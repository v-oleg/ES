using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ES.Core.Services.Abstractions;

namespace ES.EventStoreDb.Services
{
    public class StreamsSubscription : ISubscription
    {
        public Task SubscribeAsync(params Type[] aggregateProjectors)
        {
            throw new NotImplementedException();
        }
    }
}
