namespace ES.Core.Services.Abstractions;

public interface IProjectorFactory
{
    Projector Create(string projector);
    Projector Create(Type projectorType);
}