using System.Reflection;
using ES.Core.Attributes;
using ES.Core.Services.Abstractions;

namespace ES.Core.Services;

internal class ProjectorInformation : IProjectorInformation
{
    public Type AggregateProjectorType { get; set; }
    public string AggregateProjector { get; }
    public string AggregateType { get; }
    public string Service { get; }
    public string TypeFullName { get; }
    public Assembly AssemblyName { get; }
    public IReadOnlyList<string> Events { get; }

    public ProjectorInformation(Type aggregateProjectorType)
    {
        var aggregateStream = aggregateProjectorType.GetCustomAttribute<AggregateStreamAttribute>()!;
        
        AggregateProjectorType = aggregateProjectorType;
        AggregateProjector = aggregateProjectorType.Name;
        TypeFullName = aggregateProjectorType.FullName!;
        AssemblyName = aggregateProjectorType.Assembly;
        AggregateType = aggregateStream.Aggregate;
        Service = aggregateStream.Service;
        Events = (
            from methodInfo in aggregateProjectorType.GetMethods()
            from commandHandlerForAttribute in
                methodInfo.GetCustomAttributes<AggregateEventAttribute>()
            select commandHandlerForAttribute.Event).ToArray();
    }
}