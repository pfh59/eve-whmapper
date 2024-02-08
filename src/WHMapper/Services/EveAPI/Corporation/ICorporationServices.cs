using System;

namespace WHMapper.Services.EveAPI.Corporation
{
	public interface ICorporationServices
    {
        //Task<int[]> GetCorporations();
        Task<Models.DTO.EveAPI.Corporation.Corporation?> GetCorporation(int corporation_id);
    }
}

