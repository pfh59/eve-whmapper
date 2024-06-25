using System.Collections.ObjectModel;
using WHMapper.Models.Custom.Node;
using WHMapper.Models.Db;
using WHMapper.Models.DTO.EveMapper;
using WHMapper.Models.DTO.EveMapper.Enums;
using WHMapper.Models.DTO.EveMapper.EveEntity;

namespace WHMapper.Services.EveMapper
{
    public interface IEveMapperHelper
    {
        ReadOnlyCollection<WormholeType> WormholeTypes { get; }
        Task<EveSystemNodeModel> DefineEveSystemNodeModel(WHSystem wh);
        bool IsWormhole(string systemName);
        Task<EveSystemType> GetWHClass(SystemEntity whSystem);
        Task<EveSystemType> GetWHClass(string regionName, string constellationName, string systemName, float securityStatus);
        Task<bool> IsRouteViaWH(SystemEntity src, SystemEntity dst);
    }
}
