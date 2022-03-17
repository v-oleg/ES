namespace ES.Core.Services.Abstractions;

public interface IAggregateFactory
{
    Aggregate Create(string aggregateType);
    Aggregate Create(Type aggregateType);
}