using System;
using WHMapper.Models.DTO.SDE;

namespace WHMapper.Services.SDE
{
	public interface ISDEServices
	{
		bool ExtractSuccess {get; }
		Task<bool> IsNewSDEAvailable();
    
		Task<bool> DownloadSDE();
		Task<bool> ExtractSDE();
	 	Task<bool> Import();
		Task<bool> ClearCache();
		Task<bool> ClearSDERessources();
		Task<IEnumerable<SDESolarSystem>?> GetSolarSystemList();
		Task<IEnumerable<SolarSystemJump>?> GetSolarSystemJumpList();
		Task<IEnumerable<SDESolarSystem>?> SearchSystem(string value);
		Task<SDESolarSystem?> SearchSystemById(int value);
	}
}

