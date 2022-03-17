namespace ES.Core.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class AggregateEventHandlerAttribute : Attribute
{
    public string EventName { get; private set; }

    public AggregateEventHandlerAttribute(string eventName)
    {
        EventName = eventName;
    }
}