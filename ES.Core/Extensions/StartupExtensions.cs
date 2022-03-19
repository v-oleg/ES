using System.Reflection;
using System.Runtime.CompilerServices;
using ES.Core.Attributes;
using ES.Core.ConfigSettings;
using ES.Core.Services;
using ES.Core.Services.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly:InternalsVisibleTo("ES.EventStoreDb")]
namespace ES.Core.Extensions;

public static class StartupExtensions
{
    public static void AddAggregates(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<IAggregateFactory, AggregateFactory>();
        services.AddScoped<IProjectorFactory, ProjectorFactory>();
        services.AddScoped<ICommandHandler, CommandHandler>();
        services.AddTransient<IEventCreator, EventCreator>();
        services.AddTransient<IAggregateEventCreator, AggregateEventCreator>();
        
        services.Configure<ServiceOptions>(config.GetSection(ServiceOptions.ServiceSection));
        
        RegisterAggregateInformation(services);
    }
    
    public static void AddProjectors(this IServiceCollection services)
    {
        RegisterProjectorInformation(services);
    }

    #region Helpers

    private static void RegisterAggregateInformation(IServiceCollection services)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var aggregates = assembly.GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(Aggregate)));
            foreach (var aggregate in aggregates)
            {
                services.AddScoped(aggregate);
                services.AddSingleton(typeof(IAggregateInformation), new AggregateInformation(aggregate));
            }
        }
    }
    
    private static void RegisterProjectorInformation(IServiceCollection services)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var projectors = assembly.GetTypes()
                .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(Projector)) &&
                            t.GetCustomAttribute<IgnoreAttribute>() == null);
            foreach (var projector in projectors)
            {
                services.AddScoped(projector);
                services.AddSingleton(typeof(IProjectorInformation), new ProjectorInformation(projector));
            }
        }
    }

    #endregion
}