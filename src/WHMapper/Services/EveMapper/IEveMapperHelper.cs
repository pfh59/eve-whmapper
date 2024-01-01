using System;
using System.Collections.ObjectModel;
using WHMapper.Models.Custom.Node;
using WHMapper.Models.DTO.EveMapper.Enums;
using WHMapper.Models.Db;
using WHMapper.Models.DTO.EveAPI.Universe;
using WHMapper.Models.DTO.EveMapper;

namespace WHMapper.Services.EveMapper
{
	public interface IEveMapperHelper
	{
        public ReadOnlyCollection<WormholeType> WormholeTypes { get; }
        public Task<EveSystemNodeModel> DefineEveSystemNodeModel(WHSystem wh);
        public bool IsWorhmole(string systemName);
        public Task<EveSystemType> GetWHClass(SolarSystem whSystem);
        public Task<EveSystemType> GetWHClass(string regionName, string constellationName, string systemName,float securityStatus);

        

    }
}

