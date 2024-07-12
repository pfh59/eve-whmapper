using WHMapper.Shared.Models.Db.Enums;
using WHMapper.Shared.Repositories.WHAccesses;
using WHMapper.Shared.Repositories.WHAdmins;
using WHMapper.Shared.Services.EveAPI.Characters;

namespace WHMapper.Shared.Services.EveMapper
{
    public class EveMapperAccessHelper : IEveMapperAccessHelper
    {
        private readonly IWHAccessRepository _accessRepo;
        private readonly IWHAdminRepository _adminRepo;
        private readonly ICharacterServices _characterServices;

        public EveMapperAccessHelper(IWHAccessRepository accessRepo, IWHAdminRepository adminRepo, ICharacterServices eveCharacterServices)
        {
            _accessRepo = accessRepo;
            _adminRepo = adminRepo;
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
                var character = await _characterServices.GetCharacter(eveCharacterId);
                if (character == null)
                    return false;

                var result = userAccesses?.FirstOrDefault(x =>
                x.EveEntityId == eveCharacterId && x.EveEntity == WHAccessEntity.Character ||
                x.EveEntityId == character.CorporationId && x.EveEntity == WHAccessEntity.Corporation ||
                x.EveEntityId == character.AllianceId && x.EveEntity == WHAccessEntity.Alliance);

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
    }
}
