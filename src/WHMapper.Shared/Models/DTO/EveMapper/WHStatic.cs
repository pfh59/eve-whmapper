using WHMapper.Shared.Models.DTO.EveMapper.Enums;

namespace WHMapper.Shared.Models.DTO.EveMapper
{
    public class WHStatic
    {
        public string WHName { get; private set; }
        public EveSystemType EveSystemDestinationType { get; private set; }

        public WHStatic(string whName, EveSystemType destType)
        {
            WHName = whName;
            EveSystemDestinationType = destType;
        }
    }
}


