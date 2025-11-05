using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Dogma;

namespace WHMapper.Services.EveAPI.Dogma
{
    public interface IDogmaServices
    {
        Task<Result<int[]>> GetAttributes();
        Task<Result<Models.DTO.EveAPI.Dogma.Attribute>> GetAttribute(int attribute_id);
        Task<Result<int[]>> GetEffects();
        Task<Result<Effect>> GetEffect(int effect_id);
    }
}