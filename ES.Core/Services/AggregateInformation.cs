using System.Reflection;
using ES.Core.Attributes;
using ES.Core.Services.Abstractions;

namespace ES.Core.Services;

internal sealed class AggregateInformation : IAggregateInformation
{
    public Type Type { get; set; }
    public string AggregateType { get; }
    public string TypeFullName { get; }
    public Assembly AssemblyName { get; }
    public IReadOnlyList<string> Commands { get; }

    public AggregateInformation(Type aggregateType)
    {
        Type = aggregateType;
        AggregateType = aggregateType.Name;
        TypeFullName = aggregateType.FullName!;
        AssemblyName = aggregateType.Assembly;
        Commands = (
            from methodInfo in aggregateType.GetMethods()
            from commandHandlerForAttribute in
                methodInfo.GetCustomAttributes<AggregateCommandHandlerAttribute>()
            select commandHandlerForAttribute.CommandName).ToArray();
    }
}