using System.Text.Json.Serialization;
using WHMapper.Models.DTO.EveMapper.Enums;
using WHMapper.Models.DTO.SDE;

namespace WHMapper.Models.DTO.EveMapper.EveEntity
{
    public class SystemEntity : AEveEntity
    {
        public int ConstellationId { get; private set; }
        public float SecurityStatus { get; private set; }
        public int [] Stargates { get; private set; }

        public SystemEntity(int id, Models.DTO.EveAPI.Universe.ESISolarSystem entity)
            : base(id, entity.Name, EveEntityEnums.System)
        {
            SecurityStatus = entity.SecurityStatus;
            Stargates = entity.Stargates;
            ConstellationId = entity.ConstellationId;
        }

        [JsonConstructor]
        public SystemEntity(int id, string name,int constellationId,float securityStatus,int[] stargates)
            : base(id, name, EveEntityEnums.System)
        {
            SecurityStatus = securityStatus;
            Stargates = stargates;
            ConstellationId = constellationId;
        }
    }
}

