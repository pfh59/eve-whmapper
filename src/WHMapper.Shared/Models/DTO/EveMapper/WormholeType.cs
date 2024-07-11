using WHMapper.Shared.Models.DTO.EveMapper.Enums;

namespace WHMapper.Shared.Models.DTO.EveMapper
{
    public class WormholeType
    {
        public string Name { private set; get; }
        public EveSystemType Destination { private set; get; }
        public EveSystemType[]? Sources { private set; get; }

        public WormholeType(string name, EveSystemType dest, EveSystemType[]? srcs)
        {
            Name = name;
            Destination = dest;
            Sources = srcs;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Destination.ToString()))
                return Name;
            else
                return string.Format("{0} -> {1}", Name, Destination);

        }
    }
}

