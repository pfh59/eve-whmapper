using WHMapper.Models.DTO.EveMapper.Enums;
using WHMapper.Models.DTO.EveMapper.EveEntity;

namespace WHMapper.Models.DTO.EveMapper
{
    public class WormholeType
	{
        public string Name { private set; get; }
        public EveSystemType Destination { private set; get; }

        public float MassMax {get; private set;}
        public float JumpMassMax {get; private set;}

        /// <summary>
        /// Time in minutes
        /// 24h = 1440
        /// 18h = 1080
        /// </summary>
        public float StableTime {get;private set;}

        public WormholeType(string name, EveSystemType dest)
        {
            Name = name;
            Destination = dest;
        }

        public WormholeType(WHEntity entity)
        {
            Name = entity.Name;
            Destination = (EveSystemType)entity.SystemTypeValue;
            MassMax = entity.MassMax;
            JumpMassMax = entity.JumpMassMax;
            StableTime = entity.StableTime;
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

