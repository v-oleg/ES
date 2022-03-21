using ES.Core;
using ES.Core.Attributes;
using ES.Core.Commands;
using ES.Core.ConfigSettings;
using ES.Core.Events;
using ES.Core.Services.Abstractions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace ShipBob.Merchant.Aggregates;

public class MerchantIntegration : Aggregate
{
    private int _lastId;
    private readonly List<(int? id, bool active)> _integrations = new();

    private readonly IEventReader _eventReader;
    private readonly ServiceOptions _serviceOptions;

    public MerchantIntegration(IAggregateEventCreator aggregateEventCreator, IEventReader eventReader,
        IOptions<ServiceOptions> options) : base(aggregateEventCreator)
    {
        _eventReader = eventReader;
        _serviceOptions = options.Value;
    }

    [AggregateCommandHandler("AddMerchantIntegration")]
    public async Task AddMerchantIntegration(Command command)
    {
        await ValidateMerchantAsync(command.AggregateId);

        var integrationId = _lastId += 1;
        AddEvent(command, "MerchantIntegrationAdded", data =>
        {
            data["Id"] = integrationId;
            data["Platform"] = command.Data!["Platform"];
        });
        AddEvent(command, "MerchantIntegrationUpdated", data =>
        {
            data["Id"] = integrationId;
            data["StoreUrl"] = command.Data!["StoreUrl"];
        });

        AddEvent(command, "MerchantIntegrationTokenUpdated", data =>
        {
            data["Id"] = integrationId;
            //should de be encrypted
            data["Token"] = command.Data!["Token"];
        });
    }

    [AggregateCommandHandler("UpdateMerchantIntegration")]
    public async Task UpdateMerchantInformation(Command command)
    {
        await ValidateMerchantAsync(command.AggregateId);

        var integrationId = command.Data!["Id"]!.Value<int>();
        ValidateIntegration(integrationId);

        AddEvent(command, "MerchantIntegrationUpdated", data =>
        {
            data["Id"] = integrationId;
            data["StoreUrl"] = command.Data!["StoreUrl"];
        });
    }

    [AggregateCommandHandler("ReintegrateMerchantIntegration")]
    public async Task ReintegrateMerchantIntegration(Command command)
    {
        await ValidateMerchantAsync(command.AggregateId);

        var integrationId = command.Data!["Id"]!.Value<int>();
        ValidateIntegration(integrationId);
        
        AddEvent(command, "MerchantIntegrationTokenUpdated", data =>
        {
            data["Id"] = integrationId;
            //should de be encrypted
            data["Token"] = command.Data!["Token"];
        });
    }

    [AggregateCommandHandler("DeleteMerchantIntegration")]
    public async Task ReintegrateMerchantInformation(Command command)
    {
        await ValidateMerchantAsync(command.AggregateId);

        var integrationId = command.Data!["Id"]!.Value<int>();
        ValidateIntegration(integrationId);

        AddEvent(command, "MerchantIntegrationDeleted", data =>
        {
            data["Id"] = integrationId;
        });
    }
    
    [AggregateEventHandler("MerchantIntegrationAdded")]
    public void MerchantIntegrationAdded(AggregateEvent e)
    {
        _lastId = e.Data["Id"]!.Value<int>();
        _integrations.Add((_lastId, true));
    }
    
    [AggregateEventHandler("MerchantIntegrationDeleted")]
    public void MerchantIntegrationDeleted(AggregateEvent e)
    {
        var integration = _integrations.FirstOrDefault(u => u.id == e.Data["Id"]!.Value<int>());
        if (integration.id == null) return;
        
        _integrations.Remove(integration);
        integration.active = false;
        _integrations.Add(integration);
    }
    
    #region Helpers

    private async Task<bool> MerchantExistsAsync(Guid aggregateId)
    {
        var stream = Tools.Instance.Converter.ToAggregateIdStream(_serviceOptions.Name, nameof(Merchant), aggregateId);
        return await _eventReader.GetFirstAggregateEventOrNullsAsync(stream) != null;
    }
    
    private void ValidateIntegration(int integrationId)
    {
        var integration = _integrations.FirstOrDefault(u => u.id == integrationId);
        if (integration.id == null)
        {
            throw new AggregateException($"Integration id = {integrationId} does not exist");
        }

        if (!integration.active)
        {
            throw new AggregateException($"Integration id = {integration} is not active");
        }
    }
    
    private async Task ValidateMerchantAsync(Guid aggregateId)
    {
        if (!await MerchantExistsAsync(aggregateId))
        {
            throw new AggregateException("Merchant does not exist");
        }
    }

    #endregion
}