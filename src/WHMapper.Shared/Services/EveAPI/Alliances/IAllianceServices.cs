using WHMapper.Shared.Models.DTO.EveAPI.Alliance;

namespace WHMapper.Shared.Services.EveAPI.Alliances
{
    public interface IAllianceServices
    {
        Task<int[]?> GetAlliances();
        Task<Alliance?> GetAlliance(int alliance_id);
    }
}