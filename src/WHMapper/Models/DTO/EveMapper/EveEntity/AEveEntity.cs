using WHMapper.Models.DTO.EveMapper.Enums;

namespace WHMapper.Models.DTO.EveMapper.EveEntity
{
    public abstract class AEveEntity
    {
        public int Id { get; private set; }
        public string Name { get; private set; }

        public EveEntityEnums EntityType { get; private set; }

        public AEveEntity(int id, string name, EveEntityEnums entityType)
        {
            Id = id;
            Name = name;
            EntityType = entityType;
        }     
    }
}
