using System;
using WHMapper.Models.DTO.SDE;

namespace WHMapper.Services.SDE
{
	public interface ISDEServices
	{
		Task<IEnumerable<SDESolarSystem>> SearchSystem(string value);
	}
}

