using System.Text.Json;

namespace WHMapper.Shared.Services.Anoik
{
    public interface IAnoikDataSupplier
    {
        JsonElement GetEffects();
        JsonElement GetSystems();
        JsonElement GetWormHoles();
    }
}