namespace ES.Core.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class AggregateCommandHandlerAttribute : Attribute
{
    public string CommandName { get; private set; }

    public AggregateCommandHandlerAttribute(string commandName)
    {
        CommandName = commandName;
    }
}