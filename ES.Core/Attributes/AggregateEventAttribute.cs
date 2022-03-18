namespace ES.Core.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class AggregateEventAttribute : Attribute
{
    public string Event { get; }

    public AggregateEventAttribute(string @event)
    {
        Event = @event;
    }
}