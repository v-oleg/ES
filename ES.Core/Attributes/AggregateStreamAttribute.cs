namespace ES.Core.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class AggregateStreamAttribute : Attribute
{
    public string Service { get; }
    public string Aggregate { get; }
    
    public AggregateStreamAttribute(string service, string aggregate)
    {
        Service = service;
        Aggregate = aggregate;
    }
}