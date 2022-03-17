using Newtonsoft.Json.Linq;

namespace ES.Core.Services.Abstractions;

public interface IEventCreator
{
    JObject CreateEvent(string eventName, long? eventNumber, Action<JObject> data,
        Guid? correlationId = default, Guid? causationId = default, int eventVersion = 1,
        string? authorizedUserId = null);

    JObject CreateEvent(string eventName, long? eventNumber, JObject? data = null,
        Guid? correlationId = default, Guid? causationId = default, int eventVersion = 1,
        string? authorizedUserId = null);
}