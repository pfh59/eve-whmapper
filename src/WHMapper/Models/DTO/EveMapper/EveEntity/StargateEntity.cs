using System.Text.Json.Serialization;
using WHMapper.Models.DTO.EveAPI.Universe;
using WHMapper.Models.DTO.EveMapper.Enums;

namespace WHMapper.Models.DTO.EveMapper.EveEntity;

public class StargateEntity : AEveEntity
{
    public int DestinationId { get; private set; }
    public int SourceId { get; private set; }

    public StargateEntity(int id, Stargate entity)
        : base(id, entity.Name, EveEntityEnums.Stargate)
    {
        DestinationId = entity.Destination.SystemId;
        SourceId = entity.SystemId;
    }

    [JsonConstructor]
    public StargateEntity(int id, string name, int destinationId, int sourceId)
        : base(id, name, EveEntityEnums.Stargate)
    {
        DestinationId = destinationId;
        SourceId = sourceId;
    }

}
