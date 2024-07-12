using WHMapper.Shared.Models.DTO.Anoik;

namespace WHMapper.Shared.Services.Anoik
{
    public interface IAnoikServices
    {
        public int? GetSystemId(string systemName);
        public string? GetSystemClass(string systemName);
        public string? GetSystemEffects(string systemName);
        public Task<IEnumerable<KeyValuePair<string, string>>> GetSystemStatics(string systemName);
        public IEnumerable<KeyValuePair<string, string>> GetSystemEffectsInfos(string effectName, string systemClass);
        public Task<IEnumerable<WormholeTypeInfo>> GetWormholeTypes();
    }
}
