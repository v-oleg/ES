using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("ES.EventStoreDb")]
namespace ES.Core.Services.Abstractions;

internal interface ISubscription
{
    Task SubscribeAsync(params Type[] projectors);
}