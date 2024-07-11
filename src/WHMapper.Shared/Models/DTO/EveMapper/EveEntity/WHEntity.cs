using System.Text.Json.Serialization;
using WHMapper.Models.DTO.EveAPI.Universe;
using WHMapper.Models.DTO.EveMapper.Enums;

namespace WHMapper.Models.DTO.EveMapper.EveEntity;

public class WHEntity : AEveEntity
{    private const int DOGMA_ATTR_TARGET_SYSTEM_CLASS_FOR_WORMHOLES_ID = 1381;
    //private const int DOGMA_ATTR_DISTRIBUTION_ID_OF_TARGET_WORMHOLE_DISTRIBUTION_ID = 1457;
    
    public float SystemTypeValue { get; private set; }

    public WHEntity(int id, WHMapper.Models.DTO.EveAPI.Universe.Type type)
        : base(id, type.Name[9..].Trim(), EveEntityEnums.Wormhole)
    {
        SystemTypeValue = 0;
        if (type.DogmaAttributes != null)
            SystemTypeValue = type.DogmaAttributes.Where(x => x.AttributeId == DOGMA_ATTR_TARGET_SYSTEM_CLASS_FOR_WORMHOLES_ID).Select(x => x.Value).FirstOrDefault(0);
    }

    [JsonConstructor]
    public WHEntity(int id, string name,float systemTypeValue)
        : base(id, name, EveEntityEnums.Wormhole)
    {
        SystemTypeValue = systemTypeValue;
    }

}
