using System;
using WHMapper.Models.DTO.SDE;

namespace WHMapper.Services.SDE
{
	public interface ISDEServices
	{
		public bool ExtractSuccess {get; }
		public bool IsNewSDEAvailable();
        Task<IEnumerable<SDESolarSystem>?> SearchSystem(string value);
	}
}

