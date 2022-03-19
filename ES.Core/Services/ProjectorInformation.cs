using System.Reflection;
using ES.Core.Attributes;
using ES.Core.Services.Abstractions;

namespace ES.Core.Services;

internal class ProjectorInformation : IProjectorInformation
{
    public Type ProjectorType { get; set; }
    public string Projector { get; }
    public string? AggregateType { get; }
    public string? Service { get; }
    public string TypeFullName { get; }
    public Assembly AssemblyName { get; }
    public IReadOnlyList<string> Events { get; }

    public ProjectorInformation(Type projectorType)
    {
        var stream = projectorType.GetCustomAttribute<AggregateStreamAttribute>();

        ProjectorType = projectorType;
        Projector = projectorType.Name;
        TypeFullName = projectorType.FullName!;
        AssemblyName = projectorType.Assembly;
        AggregateType = stream?.Aggregate;
        Service = stream?.Service;
        Events = (
            from methodInfo in projectorType.GetMethods()
            from commandHandlerForAttribute in
                methodInfo.GetCustomAttributes<AggregateEventAttribute>()
            select commandHandlerForAttribute.Event).ToArray();
    }
}