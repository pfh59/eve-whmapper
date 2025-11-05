using WHMapper.Models.DTO;
using WHMapper.Models.DTO.EveAPI.Corporation;

namespace WHMapper.Services.EveAPI.Corporations
{
    public interface ICorporationServices
    {
        Task<Result<Corporation>> GetCorporation(int corporation_id);
    }
}