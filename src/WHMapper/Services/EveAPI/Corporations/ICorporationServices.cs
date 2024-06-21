using WHMapper.Models.DTO.EveAPI.Corporation;

namespace WHMapper.Services.EveAPI.Corporations
{
    public interface ICorporationServices
    {
        Task<Corporation?> GetCorporation(int corporation_id);
    }
}