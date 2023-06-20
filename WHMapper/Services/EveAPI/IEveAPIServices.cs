using WHMapper.Services.EveAPI.Alliance;
using WHMapper.Services.EveAPI.Character;
using WHMapper.Services.EveAPI.Corporation;
using WHMapper.Services.EveAPI.Location;
using WHMapper.Services.EveAPI.Search;
using WHMapper.Services.EveAPI.Universe;
using WHMapper.Services.EveAPI.UserInterface;

namespace WHMapper.Services.EveAPI
{
    public interface IEveAPIServices
    {
        ILocationServices LocationServices { get; }
        IUniverseServices UniverseServices { get; }
        IUserInterfaceServices UserInterfaceServices { get; }
        IAllianceServices AllianceServices { get; }
        ICorporationServices CorporationServices {get;}
        ICharacterServices CharacterServices { get; }
        ISearchServices SearchServices { get; }
    }
}
