using WHMapper.Shared.Models.DTO.EveAPI.Assets;

namespace WHMapper.Shared.Services.EveAPI.Assets
{
    public interface IAssetsServices
    {

        Task<ICollection<Asset>?> GetMyAssets(int page = 1);
        Task<ICollection<Asset>?> GetCharacterAssets(int character_id, int page = 1);
    }
}
