namespace ES.Core.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class AggregateCommandHandlerAttribute : Attribute
{
    public string CommandName { get; }

    public AggregateCommandHandlerAttribute(string commandName)
    {
        CommandName = commandName;
    }
}