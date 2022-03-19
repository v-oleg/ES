using ES.Core.Extensions;
using EventStore.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ES.Core.Services.Abstractions;
using ES.EventStoreDb.ConfigSettings;
using ES.EventStoreDb.Services;

namespace ES.EventStoreDb.Extensions;

public static class StartupExtensions
{
    public static void StartUp(this IServiceCollection services, IConfiguration config,
        params Type[] projectors)
    {
        services.AddAggregates(config);
        services.AddProjectors();

        //TODO get connection string from AKV or AKS secrets
        services.Configure<EventStoreOptions>(config.GetSection(EventStoreOptions.EventStoreSection));

        
        services.AddSingleton<IEventStoreClientService<EventStoreClient>, EventStoreClientService>();
        services.AddScoped<IEventReader, EventReader>();
        services.AddScoped<IEventWriter, EventWriter>();

        services.AddScoped<ISubscription, AllStreamsSubscription>();
        services.AddScoped<ISubscription, AggregateSubscription>();

        var subscriptions = services.BuildServiceProvider().GetServices<ISubscription>();
        foreach (var subscription in subscriptions)
        {
            subscription.SubscribeAsync(projectors);
        }
    }
}