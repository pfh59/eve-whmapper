using System;
using YamlDotNet.Serialization;

namespace WHMapper.Models.DTO.SDE
{
	public class SDESolarSystem
	{

        public bool Border { get; set; }
        public IEnumerable<double> Center { get; set; }
        public bool corridor { get; set; }

        public IEnumerable<int>  DisallowedAnchorCategories { get; set; }

        public bool Fringe { get; set; }

        public bool Hub { get; set; }

        public bool International { get; set; }

        public float Luminosity { get; set; }

        public IEnumerable<double> Max { get; set; }

        public IEnumerable<double> Min { get; set; }
       
        //TODO implement SDEPlanet object
        public IDictionary<int,object> Planets { get; set; }

        public double Radius { get; set; }

        public bool Regional { get; set; }

        //Only on WH
        public SDESecondarySun SecondarySun { get; set; }
        
        public float Security { get; set; }

        public string SecurityClass { get; set; }

        public int SolarSystemID { get;  set; }

        public int solarSystemNameID { get; set; }

        //TODO implement SDEStar object
        public object Star { get; set; }

        //TODO implement SDEStargate object
        public IDictionary<int, object> Stargates { get; set; }

        public int SunTypeID { get; set; }

        //Unknow type?
        public object VisualEffect { get; set; }

        //present in 0.0 solar system????
        public int WormholeClassID { get; set; }
        
        [YamlIgnore]
        public string Name { get;  set; }

        public SDESolarSystem() { }
        /*
        public SDESolarSystem(int soloarSystemId,string name)
		{
			SolarSystemID = soloarSystemId;
            Name = name;

        }*/
	}
}

