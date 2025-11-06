using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Alliance;

namespace WHMapper.Services.EveAPI.Alliances
{
    public interface IAllianceServices
    {
        Task<Result<int[]>> GetAlliances();
        Task<Result<Alliance>> GetAlliance(int alliance_id);
    }
}