using ES.Core.ConfigSettings;
using ES.Core.Services.Abstractions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace ES.Core.Services;

public class EventCreator : IEventCreator
{
    private readonly ServiceOptions _serviceOptions;

    public EventCreator(IOptions<ServiceOptions> options)
    {
        _serviceOptions = options.Value;
    }

    public JObject CreateEvent(string eventName, long? eventNumber, Action<JObject> data, Guid? correlationId = default,
        Guid? causationId = default, int eventVersion = 1, string? authorizedUserId = null)
    {
        var d = new JObject();
        data.Invoke(d);
        var @event = new JObject
        {
            ["eventType"] = "Event",
            ["eventId"] = Guid.NewGuid(),
            ["eventNumber"] = eventNumber,
            ["data"] = d,
            ["eventName"] = eventName,
            ["eventDate"] = DateTime.UtcNow,
            ["eventVersion"] = eventVersion,
            ["correlationId"] = correlationId,
            ["causationId"] = causationId,
            ["authorizedUserId"] = authorizedUserId,
            ["service"] = _serviceOptions.Name
        };
        
        return @event;
    }

    public JObject CreateEvent(string eventName, long? eventNumber, JObject? data = null, Guid? correlationId = default,
        Guid? causationId = default, int eventVersion = 1, string? authorizedUserId = null)
    {
        return new JObject
        {
            ["eventType"] = "Event",
            ["eventId"] = Guid.NewGuid(),
            ["eventNumber"] = eventNumber,
            ["data"] = data ?? new JObject(),
            ["eventName"] = eventName,
            ["eventDate"] = DateTime.UtcNow,
            ["eventVersion"] = eventVersion,
            ["correlationId"] = correlationId,
            ["causationId"] = causationId,
            ["authorizedUserId"] = authorizedUserId,
            ["service"] = _serviceOptions.Name
        };
    }
}