using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("ES.EventStoreDb")]
namespace ES.Core.Services.Abstractions;

internal interface IEventStoreClientService<out T>
{
    T EventStoreClient { get; }
}