using System.Text.Json;

namespace WHMapper.Services.Anoik
{
    public interface IAnoikDataSupplier
    {
        JsonElement GetEffects();
        JsonElement GetSystems();
        JsonElement GetWormHoles();
    }
}