using WHMapper.Services.EveAPI.Location;
using WHMapper.Services.EveAPI.Universe;

namespace WHMapper.Services.EveAPI
{
    public interface IEveAPIServices
    {
        ILocationServices LocationServices { get; }
        IUniverseServices UniverseServices { get; }
    }
}
