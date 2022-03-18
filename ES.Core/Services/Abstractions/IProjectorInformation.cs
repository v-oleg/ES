using System.Reflection;

namespace ES.Core.Services.Abstractions;

internal interface IProjectorInformation
{
    Type AggregateProjectorType { get; set; }
    string AggregateProjector { get; }
    string AggregateType { get; }
    string Service { get; }
    string TypeFullName { get; }
    Assembly AssemblyName { get; }
    IReadOnlyList<string> Events { get; }
}