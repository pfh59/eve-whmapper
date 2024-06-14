using System;
using WHMapper.Models.DTO.SDE;

namespace WHMapper.Services.SDE
{
	public interface ISDEServices
	{
		const string REDIS_SOLAR_SYSTEM_JUMPS_KEY = "solarysystemjumps:list";
        const string REDIS_SDE_SOLAR_SYSTEMS_KEY = "solarsystems:list";

		bool ExtractSuccess {get; }
		Task<bool> IsNewSDEAvailable();
    
		Task<bool> DownloadSDE();
		Task<bool> ExtractSDE();
	 	Task<bool> Import();
		Task<bool> ClearCache();
		Task<bool> ClearSDEResources();
		Task<IEnumerable<SDESolarSystem>?> GetSolarSystemList();
		Task<IEnumerable<SolarSystemJump>?> GetSolarSystemJumpList();
		Task<IEnumerable<SDESolarSystem>?> SearchSystem(string value);
		Task<SDESolarSystem?> SearchSystemById(int value);
	}
}

