using System.Collections.ObjectModel;
using WHMapper.Shared.Models.Custom.Node;
using WHMapper.Shared.Models.Db;
using WHMapper.Shared.Models.DTO.EveMapper;
using WHMapper.Shared.Models.DTO.EveMapper.Enums;
using WHMapper.Shared.Models.DTO.EveMapper.EveEntity;

namespace WHMapper.Shared.Services.EveMapper
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
