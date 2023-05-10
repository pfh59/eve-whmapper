using WHMapper.Services.EveAPI.Location;
using WHMapper.Services.EveAPI.Universe;
using WHMapper.Services.EveAPI.UserInterface;

namespace WHMapper.Services.EveAPI
{
    public interface IEveAPIServices
    {
        ILocationServices LocationServices { get; }
        IUniverseServices UniverseServices { get; }
        IUserInterfaceServices UserInterfaceServices { get; }
    }
}
