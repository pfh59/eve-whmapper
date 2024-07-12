using System.Text.Json.Serialization;
using WHMapper.Shared.Models.DTO.EveAPI.Universe;
using WHMapper.Shared.Models.DTO.EveMapper.Enums;

namespace WHMapper.Shared.Models.DTO.EveMapper.EveEntity;

public class GroupEntity : AEveEntity
{
    public int[] Types { get; private set; }
    public int CategoryId { get; private set; }
    public GroupEntity(int id, Group group)
        : base(id, group.Name, EveEntityEnums.Group)
    {
        Types = group.Types;
        CategoryId = group.CategoryId;
    }

    [JsonConstructor]
    public GroupEntity(int id, string name, int[] types, int categoryId)
        : base(id, name, EveEntityEnums.Group)
    {
        Types = types;
        CategoryId = categoryId;

    }


}
