using WHMapper.Models.Db.Enums;
using WHMapper.Repositories.WHAccesses;
using WHMapper.Repositories.WHAdmins;
using WHMapper.Repositories.WHMaps;
using WHMapper.Services.EveAPI.Characters;

namespace WHMapper.Services.EveMapper
{
    public class EveMapperAccessHelper : IEveMapperAccessHelper
    {
        private readonly IWHAccessRepository _accessRepo;
        private readonly IWHAdminRepository _adminRepo;
        private readonly ICharacterServices _characterServices;
        private readonly IWHMapRepository _mapRepo;

        public EveMapperAccessHelper(IWHAccessRepository accessRepo, IWHAdminRepository adminRepo,IWHMapRepository mapRepo, ICharacterServices eveCharacterServices)
        {
            _accessRepo = accessRepo;
            _adminRepo = adminRepo;
            _mapRepo = mapRepo;
            _characterServices = eveCharacterServices;
        }

        public async Task<bool> IsEveMapperUserAccessAuthorized(int eveCharacterId)
        {
            var userAccesses = await _accessRepo.GetAll();

            //If there is no user access registered return true, this is the probably the first user using the tool. 
            if (userAccesses?.Count() == 0)
            {
                return true;
            }
            else
            {
                var characterResult = await _characterServices.GetCharacter(eveCharacterId);
                if (!characterResult.IsSuccess || characterResult.Data == null)
                    return false;
                var character = characterResult.Data;

                var result = userAccesses?.FirstOrDefault(x => 
                (x.EveEntityId == eveCharacterId && x.EveEntity == WHAccessEntity.Character) || 
                (x.EveEntityId == character.CorporationId && x.EveEntity == WHAccessEntity.Corporation) || 
                (x.EveEntityId == character.AllianceId && x.EveEntity == WHAccessEntity.Alliance));

                if (result == null)
                    return false; //TODO: check alliance and corpo and add db methodes
                else
                    return true;
            }
        }

        public async Task<bool> IsEveMapperAdminAccessAuthorized(int eveCharacterId)
        {
            var adminAccesses = await _adminRepo.GetAll();

            //If there is no user access registered return true, this is the probably the first user using the tool. 
            if (adminAccesses?.Count() == 0)
            {
                return true;
            }
            else
            {
                var result = adminAccesses?.FirstOrDefault(x => x.EveCharacterId == eveCharacterId);

                if (result == null)
                    return false;
                else
                    return true;
            }
        }

        public async Task<bool> IsEveMapperMapAccessAuthorized(int eveCharacterId, int mapId)
        {
            var userAccesses = await _accessRepo.GetAll();

            //If there is no user access registered return true, this is the probably the first user using the tool. 
            if (userAccesses?.Count() == 0)
            {
                return true;
            }
            else
            {
                var characterResult = await _characterServices.GetCharacter(eveCharacterId);
                if (!characterResult.IsSuccess || characterResult.Data == null)
                    return false;
                var character = characterResult.Data;

                var mapAccess = await _mapRepo.GetMapAccesses(mapId);
                
                if (mapAccess == null || !mapAccess.Any())
                    return false;
                else
                {
                    if(mapAccess.FirstOrDefault(x => 
                        (x.EveEntityId == eveCharacterId && x.EveEntity == WHAccessEntity.Character) || 
                        (x.EveEntityId == character.CorporationId && x.EveEntity == WHAccessEntity.Corporation) || 
                        (x.EveEntityId == character.AllianceId && x.EveEntity == WHAccessEntity.Alliance)) == null)
                        return false;
                    else
                        return true;
                }
            }
        }
    }
}
