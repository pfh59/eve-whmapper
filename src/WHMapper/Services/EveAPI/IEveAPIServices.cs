using WHMapper.Services.EveAPI.Alliances;
using WHMapper.Services.EveAPI.Assets;
using WHMapper.Services.EveAPI.Characters;
using WHMapper.Services.EveAPI.Corporations;
using WHMapper.Services.EveAPI.Dogma;
using WHMapper.Services.EveAPI.Locations;
using WHMapper.Services.EveAPI.Routes;
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
        ICorporationServices CorporationServices { get; }
        ICharacterServices CharacterServices { get; }
        ISearchServices SearchServices { get; }
        IDogmaServices DogmaServices { get; }
        IRouteServices RouteServices { get; }
        IAssetsServices AssetsServices { get; }
    }
}
