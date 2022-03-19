using ES.Core.Services.Abstractions;

namespace ES.Core.Services;

internal sealed class ProjectorFactory : IProjectorFactory
{
    private readonly Dictionary<string, IProjectorInformation> _aggregateProjectors;
    private readonly IServiceProvider _serviceProvider;

    public ProjectorFactory(IEnumerable<IProjectorInformation> aggregateProjectors, IServiceProvider serviceProvider)
    {
        _aggregateProjectors = aggregateProjectors.ToDictionary(k => k.Projector);
        _serviceProvider = serviceProvider;
    }

    public Projector Create(string projector) =>
        (Projector) _serviceProvider.GetService(_aggregateProjectors[projector]
            .ProjectorType)!;

    public Projector Create(Type projectorType) =>
        (Projector) _serviceProvider.GetService(projectorType)!;
}