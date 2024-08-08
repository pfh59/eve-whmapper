using System.Text.Json.Serialization;
using WHMapper.Models.DTO.EveAPI.Universe;
using WHMapper.Models.DTO.EveMapper.Enums;

namespace WHMapper.Models.DTO.EveMapper.EveEntity;

public class WHEntity : AEveEntity
{    private const int DOGMA_ATTR_TARGET_SYSTEM_CLASS_FOR_WORMHOLES_ID = 1381;
     private const int DOGMA_ATTR_WORMHOLE_MAX_STABLE_MASS = 1383;
     private const int DOGMA_ATTR_WORMHOLE_MAX_JUMP_MASS = 1385;
     private const int DOGMA_ATTR_WORMHOLE_MAX_STABLE_TIME = 1382; //unit8id 3 value 1440 = 24h???


    public float SystemTypeValue { get; private set; }
    public float MassMax {get; private set;}
    public float JumpMassMax {get; private set;}

    /// <summary>
    /// Time in minutes
    /// 24h = 1440
    /// 18h = 1080
    /// </summary>
    public float StableTime {get;private set;}

    public WHEntity(int id, WHMapper.Models.DTO.EveAPI.Universe.Type type)
        : base(id, type.Name[9..].Trim(), EveEntityEnums.Wormhole)
    {
        SystemTypeValue = 0;
        MassMax = 0;
        JumpMassMax = 0;
        StableTime = 0;
        if (type.DogmaAttributes != null)
        {
            SystemTypeValue = type.DogmaAttributes.Where(x => x.AttributeId == DOGMA_ATTR_TARGET_SYSTEM_CLASS_FOR_WORMHOLES_ID).Select(x => x.Value).FirstOrDefault(0);
            MassMax = type.DogmaAttributes.Where(x => x.AttributeId == DOGMA_ATTR_WORMHOLE_MAX_STABLE_MASS).Select(x => x.Value).FirstOrDefault(0);
            JumpMassMax = type.DogmaAttributes.Where(x => x.AttributeId == DOGMA_ATTR_WORMHOLE_MAX_JUMP_MASS).Select(x => x.Value).FirstOrDefault(0);
            StableTime = type.DogmaAttributes.Where(x => x.AttributeId == DOGMA_ATTR_WORMHOLE_MAX_STABLE_TIME).Select(x => x.Value).FirstOrDefault(0);
        }
    }

    [JsonConstructor]
    public WHEntity(int id, string name,float systemTypeValue, float massMax, float jumpMassMax, float stableTime)
        : base(id, name, EveEntityEnums.Wormhole)
    {
        SystemTypeValue = systemTypeValue;
        MassMax = massMax;
        JumpMassMax = jumpMassMax;
        StableTime = stableTime;
    }


}
