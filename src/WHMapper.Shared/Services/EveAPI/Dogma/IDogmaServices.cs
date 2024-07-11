using WHMapper.Shared.Models.DTO.EveAPI.Dogma;

namespace WHMapper.Shared.Services.EveAPI.Dogma
{
    public interface IDogmaServices
    {
        Task<int[]?> GetAttributes();
        Task<Models.DTO.EveAPI.Dogma.Attribute?> GetAttribute(int attribute_id);
        Task<int[]?> GetEffects();
        Task<Effect?> GetEffect(int effect_id);
    }
}
