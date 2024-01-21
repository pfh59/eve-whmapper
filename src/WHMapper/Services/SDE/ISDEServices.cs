using System;
using WHMapper.Models.DTO.SDE;

namespace WHMapper.Services.SDE
{
	public interface ISDEServices
	{
		bool ExtractSuccess {get; }
		bool IsNewSDEAvailable();
    
		Task<bool> DownloadSDE();
		bool ExtractSDE();
	 	Task<bool> Import();
		Task<bool> ClearCache();
		Task<IEnumerable<SDESolarSystem>?> GetSolarSystemList();
		Task<IEnumerable<SolarSystemJump>?> GetSolarSystemJumpList();
		Task<IEnumerable<SDESolarSystem>?> SearchSystem(string value);
	}
}

