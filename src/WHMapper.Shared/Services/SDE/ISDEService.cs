using WHMapper.Shared.Models.DTO.SDE;

namespace WHMapper.Shared.Services.SDE
{
    public interface ISDEService
    {
        Task<IEnumerable<SDESolarSystem>?> GetSolarSystemList();
        Task<IEnumerable<SolarSystemJump>?> GetSolarSystemJumpList();
        Task<IEnumerable<SDESolarSystem>?> SearchSystem(string value);
        Task<SDESolarSystem?> SearchSystemById(int value);
    }
}