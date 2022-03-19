namespace ES.Core.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class StreamAttribute : Attribute
{
    public string Service { get; }
    public string Event { get; }
    
    public StreamAttribute(string service, string @event)
    {
        Service = service;
        Event = @event;
    }
}