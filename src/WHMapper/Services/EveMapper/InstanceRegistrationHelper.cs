using WHMapper.Models.Db;
using WHMapper.Models.Db.Enums;
using WHMapper.Models.DTO;
using WHMapper.Services.EveAPI.Characters;

namespace WHMapper.Services.EveMapper;

public class InstanceRegistrationHelper : IInstanceRegistrationHelper
{
    private readonly IEveMapperUserManagementService _userManagement;
    private readonly ICharacterServices _characterServices;
    private readonly IEveMapperService _eveMapperService;
    private readonly IEveMapperInstanceService _instanceService;
    private readonly ILogger<InstanceRegistrationHelper> _logger;

    public InstanceRegistrationHelper(
        IEveMapperUserManagementService userManagement,
        ICharacterServices characterServices,
        IEveMapperService eveMapperService,
        IEveMapperInstanceService instanceService,
        ILogger<InstanceRegistrationHelper> logger)
    {
        _userManagement = userManagement;
        _characterServices = characterServices;
        _eveMapperService = eveMapperService;
        _instanceService = instanceService;
        _logger = logger;
    }

    public async Task<InstanceRegistrationContext> LoadRegistrationContextAsync(string? clientId)
    {
        var context = new InstanceRegistrationContext();

        if (string.IsNullOrEmpty(clientId))
            return context;

        var primaryAccount = await _userManagement.GetPrimaryAccountAsync(clientId);
        if (primaryAccount == null)
            return context;

        context.IsAuthenticated = true;
        context.CharacterId = primaryAccount.Id;

        var characterResult = await _characterServices.GetCharacter(context.CharacterId);
        if (characterResult.IsSuccess && characterResult.Data != null)
        {
            context.CharacterInfo = characterResult.Data;
            context.CharacterName = characterResult.Data.Name ?? string.Empty;

            if (characterResult.Data.CorporationId > 0)
            {
                var corp = await _eveMapperService.GetCorporation(characterResult.Data.CorporationId);
                context.CorporationName = corp?.Name ?? "Unknown Corporation";
            }

            if (characterResult.Data.AllianceId > 0)
            {
                var alliance = await _eveMapperService.GetAlliance(characterResult.Data.AllianceId);
                context.AllianceName = alliance?.Name ?? "Unknown Alliance";
            }
        }

        var existingInstances = await _instanceService.GetAdministeredInstancesAsync(context.CharacterId);
        if (existingInstances?.Any() == true)
        {
            context.AlreadyHasInstance = true;
            context.ExistingInstanceId = existingInstances.First().Id;
        }

        return context;
    }

    public async Task<WHInstance?> RegisterInstanceAsync(
        InstanceRegistrationContext context,
        string instanceName,
        string? description,
        WHAccessEntity ownerType)
    {
        int ownerEntityId;
        string ownerEntityName;

        switch (ownerType)
        {
            case WHAccessEntity.Character:
                ownerEntityId = context.CharacterId;
                ownerEntityName = context.CharacterName;
                break;
            case WHAccessEntity.Corporation:
                if (context.CharacterInfo == null || context.CharacterInfo.CorporationId <= 0)
                    throw new InvalidOperationException("Corporation information not available");
                ownerEntityId = context.CharacterInfo.CorporationId;
                ownerEntityName = context.CorporationName;
                break;
            case WHAccessEntity.Alliance:
                if (context.CharacterInfo == null || context.CharacterInfo.AllianceId <= 0)
                    throw new InvalidOperationException("Alliance information not available");
                ownerEntityId = context.CharacterInfo.AllianceId;
                ownerEntityName = context.AllianceName;
                break;
            default:
                throw new InvalidOperationException("Invalid owner type selected");
        }

        if (!await _instanceService.CanRegisterAsync(ownerEntityId))
            throw new InvalidOperationException("An instance already exists for this entity");

        return await _instanceService.CreateInstanceAsync(
            instanceName,
            string.IsNullOrWhiteSpace(description) ? null : description,
            ownerEntityId,
            ownerEntityName,
            ownerType,
            context.CharacterId,
            context.CharacterName);
    }
}
