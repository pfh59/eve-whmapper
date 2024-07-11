using WHMapper.Shared.Models.DTO.EveAPI.Corporation;

namespace WHMapper.Shared.Services.EveAPI.Corporations
{
    public interface ICorporationServices
    {
        Task<Corporation?> GetCorporation(int corporation_id);
    }
}