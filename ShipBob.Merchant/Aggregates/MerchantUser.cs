using ES.Core;
using ES.Core.Attributes;
using ES.Core.Commands;
using ES.Core.ConfigSettings;
using ES.Core.Events;
using ES.Core.Services.Abstractions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace ShipBob.Merchant.Aggregates;

public class MerchantUser : Aggregate
{
    private int _lastId;
    private readonly List<(int? id, bool owner, bool active)> _users = new();

    private readonly IEventReader _eventReader;
    private readonly ServiceOptions _serviceOptions;

    public MerchantUser(IAggregateEventCreator aggregateEventCreator, IEventReader eventReader,
        IOptions<ServiceOptions> options) : base(aggregateEventCreator)
    {
        _eventReader = eventReader;
        _serviceOptions = options.Value;
    }

    [AggregateCommandHandler("AddMerchantUser")]
    public async Task AddMerchant(Command command)
    {
        await ValidateMerchantAsync(command.AggregateId);

        var userId = _lastId += 1;
        AddEvent(command, "MerchantUserAdded", data => { data["Id"] = userId; });
        AddEvent(command, "MerchantUserInformationUpdated", data =>
        {
            data["Id"] = userId;
            data["FirstName"] = command.Data!["FirstName"];
            data["LastName"] = command.Data!["LastName"];
        });

        AddEvent(command, "MerchantUserOwnerAssigned", data =>
        {
            data["Id"] = userId;
            data["Owner"] = command.Data!["Owner"];
        });
    }

    [AggregateCommandHandler("UpdateMerchantUserInformation")]
    public async Task UpdateMerchantUserInformation(Command command)
    {
        await ValidateMerchantAsync(command.AggregateId);

        var userId = command.Data!["Id"]!.Value<int>();
        ValidateUser(userId);

        AddEvent(command, "MerchantUserInformationUpdated", data =>
        {
            data["Id"] = userId;
            data["FirstName"] = command.Data!["FirstName"];
            data["LastName"] = command.Data!["LastName"];
        });
    }

    [AggregateCommandHandler("AssignMerchantUserOwner")]
    public async Task AssignMerchantUserOwner(Command command)
    {
        await ValidateMerchantAsync(command.AggregateId);

        var userId = command.Data!["Id"]!.Value<int>();
        var user = ValidateUser(userId);

        if (!user.owner)
        {
            AddEvent(command, "MerchantUserOwnerAssigned", data =>
            {
                data["Id"] = userId;
                data["Owner"] = true;
            });
        }
    }

    [AggregateCommandHandler("UnassignMerchantUserOwner")]
    public async Task UnassignMerchantUserOwner(Command command)
    {
        await ValidateMerchantAsync(command.AggregateId);

        var userId = command.Data!["Id"]!.Value<int>();
        var user = ValidateUser(userId);

        if (_users.Count(u => u.active) == 1)
        {
            throw new AggregateException("At least one user must be owner");
        }

        if (user.owner)
        {
            AddEvent(command, "MerchantUserOwnerUnassigned", data =>
            {
                data["Id"] = userId;
                data["Owner"] = false;
            });
        }
    }

    [AggregateCommandHandler("DeleteMerchantUser")]
    public async Task DeleteMerchantUser(Command command)
    {
        await ValidateMerchantAsync(command.AggregateId);

        var userId = command.Data!["Id"]!.Value<int>();
        ValidateUser(userId);

        if (_users.Count(u => u.active) == 1)
        {
            throw new AggregateException($"User id {userId} is the only owner and can not be deleted");
        }

        AddEvent(command, "MerchantUserDeleted", data => { data["Id"] = userId; });
    }

    [AggregateEventHandler("MerchantUserAdded")]
    public void MerchantUserAdded(AggregateEvent e)
    {
        _lastId = e.Data["Id"]!.Value<int>();
        _users.Add((_lastId, false, true));
    }

    [AggregateEventHandler("MerchantUserOwnerAssigned")]
    public void AssignMerchantUserOwner(AggregateEvent e)
    {
        var user = _users.FirstOrDefault(u => u.id == e.Data["Id"]!.Value<int>());
        if (user.id == null) return;
        
        _users.Remove(user);
        user.owner = true;
        _users.Add(user);
    }

    [AggregateEventHandler("MerchantUserOwnerUnassigned")]
    public void MerchantUserOwnerAssigned(AggregateEvent e)
    {
        var user = _users.FirstOrDefault(u => u.id == e.Data["Id"]!.Value<int>());
        if (user.id == null) return;
        
        _users.Remove(user);
        user.owner = false;
        _users.Add(user);
    }

    [AggregateEventHandler("DeleteMerchantUser")]
    public void MerchantUserOwnerUnassigned(AggregateEvent e)
    {
        var user = _users.FirstOrDefault(u => u.id == e.Data["Id"]!.Value<int>());
        if (user.id == null) return;
        
        _users.Remove(user);
        user.active = false;
        _users.Add(user);
    }

    #region Helpers

    private async Task<bool> MerchantExistsAsync(Guid aggregateId)
    {
        var stream = Tools.Instance.Converter.ToAggregateIdStream(_serviceOptions.Name, nameof(Merchant), aggregateId);
        return await _eventReader.GetFirstAggregateEventOrNullsAsync(stream) != null;
    }
    
    private async Task ValidateMerchantAsync(Guid aggregateId)
    {
        if (!await MerchantExistsAsync(aggregateId))
        {
            throw new AggregateException("Merchant does not exist");
        }
    }
    
    private (int id, bool owner, bool active) ValidateUser(int userId)
    {
        var (id, owner, active) = _users.FirstOrDefault(u => u.id == userId);
        if (id == null)
        {
            throw new AggregateException($"User id = {userId} does not exist");
        }

        if (!active)
        {
            throw new AggregateException($"User id = {userId} is not active");
        }

        return (id.Value, owner, active);
    }

    #endregion
}