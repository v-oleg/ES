using ES.Core.Services.Abstractions;

namespace ES.Core.Services;

internal sealed class AggregateFactory : IAggregateFactory
{
    private readonly Dictionary<string, IAggregateInformation> _aggregates;
    private readonly IServiceProvider _serviceProvider;

    public AggregateFactory(IEnumerable<IAggregateInformation> aggregateInformations, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        
        _aggregates = aggregateInformations.ToDictionary(k => k.AggregateType);
    }

    public Aggregate Create(string aggregateType) =>
        (Aggregate) _serviceProvider.GetService(_aggregates[aggregateType].Type)!;

    public Aggregate Create(Type aggregateType) =>
        (Aggregate) _serviceProvider.GetService(aggregateType)!;
}