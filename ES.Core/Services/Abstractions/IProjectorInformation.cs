using System.Reflection;

namespace ES.Core.Services.Abstractions;

internal interface IProjectorInformation
{
    Type ProjectorType { get; set; }
    string Projector { get; }
    string? AggregateType { get; }
    string? Service { get; }
    string TypeFullName { get; }
    Assembly AssemblyName { get; }
    IReadOnlyList<string> Events { get; }
}