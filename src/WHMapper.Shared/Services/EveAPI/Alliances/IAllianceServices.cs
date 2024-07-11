using WHMapper.Models.DTO.EveAPI.Alliance;

namespace WHMapper.Services.EveAPI.Alliances
{
    public interface IAllianceServices
    {
        Task<int[]?> GetAlliances();
        Task<Alliance?> GetAlliance(int alliance_id);
    }
}