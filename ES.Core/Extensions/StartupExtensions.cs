using System.Reflection;
using ES.Core.ConfigSettings;
using ES.Core.Services;
using ES.Core.Services.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ES.Core.Extensions;

public static class StartupExtensions
{
    public static IServiceCollection AddAggregates(this IServiceCollection services, IConfiguration config,
        Assembly assembly)
    {   
        RegisterAggregateInformation(services, assembly);

        services.AddScoped<IAggregateFactory, AggregateFactory>();
        services.AddScoped<ICommandHandler, CommandHandler>();
        services.AddTransient<IEventCreator, EventCreator>();
        services.AddTransient<IAggregateEventCreator, AggregateEventCreator>();
        
        services.Configure<ServiceOptions>(config.GetSection(ServiceOptions.ServiceSection));

        return services;
    }

    #region Helpers

    private static void RegisterAggregateInformation(IServiceCollection services, Assembly assembly)
    {
        var aggregates = assembly.GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(Aggregate)));
        foreach (var aggregate in aggregates)
        {
            services.AddScoped(aggregate);
            services.AddSingleton(typeof(IAggregateInformation), new AggregateInformation(aggregate));
        }
    }

    #endregion
}