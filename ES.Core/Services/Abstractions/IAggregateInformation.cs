using System.Reflection;

namespace ES.Core.Services.Abstractions;

internal interface IAggregateInformation
{
    Type Type { get; set; }
    string AggregateType { get; }
    string TypeFullName { get; }
    Assembly AssemblyName { get; }
    IReadOnlyList<string> Commands { get; }
}