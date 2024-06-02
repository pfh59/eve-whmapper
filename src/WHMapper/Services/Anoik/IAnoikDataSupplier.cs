using System.Text.Json;

namespace WHMapper.Services.Anoik
{
    public interface IAnoikDataSupplier
    {
        JsonElement GetEffect();
        JsonElement GetSystems();
        JsonElement GetWormHoles();
    }
}