using System;
using WHMapper.Models.DTO.SDE;

namespace WHMapper.Services.SDE
{
	public interface ISDEServices
	{
		IEnumerable<SDESolarSystem> SearchSystem(string value);
	}
}

