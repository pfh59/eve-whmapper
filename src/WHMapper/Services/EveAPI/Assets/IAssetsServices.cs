using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Assets;

namespace WHMapper.Services.EveAPI.Assets
{
    public interface IAssetsServices
    {
        Task<Result<ICollection<Asset>>> GetMyAssets(int page = 1);
        Task<Result<ICollection<Asset>>> GetCharacterAssets(int character_id, int page = 1);
    }
}