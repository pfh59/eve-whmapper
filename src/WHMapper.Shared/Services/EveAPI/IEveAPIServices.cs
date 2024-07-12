using WHMapper.Services.EveAPI.UserInterface;
using WHMapper.Shared.Services.EveAPI.Alliances;
using WHMapper.Shared.Services.EveAPI.Assets;
using WHMapper.Shared.Services.EveAPI.Characters;
using WHMapper.Shared.Services.EveAPI.Corporations;
using WHMapper.Shared.Services.EveAPI.Dogma;
using WHMapper.Shared.Services.EveAPI.Locations;
using WHMapper.Shared.Services.EveAPI.Routes;
using WHMapper.Shared.Services.EveAPI.Search;
using WHMapper.Shared.Services.EveAPI.Universe;

namespace WHMapper.Shared.Services.EveAPI
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
